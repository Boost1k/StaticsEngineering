// Analysis/StaticsSolver.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StaticsEngineeringCAD.Core;
using StaticsEngineeringCAD.Elements;

namespace StaticsEngineeringCAD.Analysis
{

    public class SupportReaction
    {
        public SupportElement Support { get; set; }
        public float Rx { get; set; } // Горизонтальная реакция
        public float Ry { get; set; } // Вертикальная реакция
        public float Mz { get; set; } // Реактивный момент
    }

    public class GlobalCalculactionResult
    {
        public List<SupportReaction> SupportReactions { get; set; }
        public bool IsSolved { get; set; } = false;
        public bool IsStaticallyDeterminate { get; set; } = true;
        public string Message { get; set; } = string.Empty;

        public GlobalCalculactionResult()
        {
            SupportReactions = new List<SupportReaction>();
        }
    }


    public class StaticsSolver
    {
        public GlobalCalculactionResult CalculateReactions(Scene scene)
        {
            var results = new GlobalCalculactionResult();

            // 1. Найти все балки и действующие на них силы.
            BeamElement beam = scene.Elements.OfType<BeamElement>().FirstOrDefault();
            if (beam == null)
            {
                results.Message = "Балка не найдена на схеме.";
                return results;
            }

            // 2. Найти все опоры, привязанные к этой балке.
            var supportsOnBeam = scene.Elements.OfType<SupportElement>()
                                     .Where(s => s.AttachedToNode?.ParentBeam == beam)
                                     .OrderBy(s => s.Location.X)
                                     .ToList();

            if (supportsOnBeam.Count == 0)
            {
                results.Message = "Опоры, привязанные к балке, не найдены.";
                return results;
            }

            // 3. Найти все силы, действующие на балку.
            var forcesOnBeam = scene.Elements.OfType<ForceElement>()
                                   .Where(f => (f.AttachedToNode?.ParentBeam == beam) || IsForceNearBeam(f, beam))
                                   .ToList();

            // --- Подсчет суммы внешних сил и моментов от них ---
            float totalExternalFx = 0;
            float totalExternalFy = 0;

            PointF momentReferencePoint = supportsOnBeam.First().Location;
            float sumMomentsAboutRefPoint = 0;

            foreach (var force in forcesOnBeam)
            {
                float angleRad = force.AngleDegrees * (float) Math.PI / 180f;
                float Fx = force.Magnitude * (float) Math.Cos(angleRad);
                float Fy = force.Magnitude * (float) Math.Sin(angleRad);

                totalExternalFx += Fx;
                totalExternalFy += Fy;

                // Момент относительно momentReferencePoint
                // M = Fy * (x_force - x_ref) - Fx * (y_force - y_ref)
                // Для балок часто (y_force - y_ref) считают равным 0
                sumMomentsAboutRefPoint += Fy * (force.Location.X - momentReferencePoint.X);
                // Если хотим учесть момент от Fx из-за разницы высот:
                // sumMomentsAboutRefPoint -= Fx * (force.Location.Y - momentReferencePoint.Y);
            }

            // --- Попытка решить систему на основе ТИПА и КОЛИЧЕСТВА опор ---
            
            if (supportsOnBeam.Count == 2)
            {
                SupportElement s1 = supportsOnBeam[0];
                SupportElement s2 = supportsOnBeam[1];

                var r1 = new SupportReaction { Support = s1 };
                var r2 = new SupportReaction { Support = s2 };

                // Случай 1: Шарнирно-неподвижная (Pinned) + Шарнирно-подвижная (Roller)
                if ((s1.Type == SupportType.Pinned && s2.Type == SupportType.Roller) ||
                    (s1.Type == SupportType.Roller && s2.Type == SupportType.Pinned))
                {
                    SupportElement pinned = (s1.Type == SupportType.Pinned) ? s1 : s2;
                    SupportElement roller = (s1.Type == SupportType.Roller) ? s1 : s2;
                    SupportReaction pinnedResult = (pinned == s1) ? r1 : r2;
                    SupportReaction rollerResult = (roller == s1) ? r1 : r2;

                    // 1. ΣFx = 0 => PinnedRx + totalExternalFx = 0
                    pinnedResult.Rx = -totalExternalFx;

                    // 2. ΣM_pinned = 0 => RollerRy * (x_roller - x_pinned) + Моменты_от_сил_относительно_pinned = 0
                    float armRoller = roller.Location.X - pinned.Location.X;
                    if (Math.Abs(armRoller) < 0.01f)
                    {
                        results.Message = "Опоры Pinned и Roller на одной вертикали. Система некорректна.";
                        results.IsStaticallyDeterminate = false; return results;
                    }

                    // Моменты от внешних сил относительно точки опоры 'pinned'
                    float sumMomentsAboutPinned = 0;
                    foreach (var force in forcesOnBeam)
                    {
                        float angleRad = force.AngleDegrees * (float)Math.PI / 180f;
                        sumMomentsAboutPinned += (force.Magnitude * (float)Math.Sin(angleRad)) * (force.Location.X - pinned.Location.X);
                        // sumMomentsAboutPinned -= (force.Magnitude * (float)Math.Cos(angleRad)) * (force.Location.Y - pinned.Location.Y);
                    }

                    rollerResult.Ry = -sumMomentsAboutPinned / armRoller;
                    rollerResult.Rx = 0; // Roller не имеет горизонтальной реакции (в простом случае)

                    // 3. ΣFy = 0 => PinnedRy + RollerRy + totalExternalFy = 0
                    pinnedResult.Ry = -totalExternalFy - rollerResult.Ry;

                    results.SupportReactions.Add(pinnedResult);
                    results.SupportReactions.Add(rollerResult);
                    results.IsSolved = true;
                }
                // Случай 2: Две шарнирно-подвижные опоры (Roller + Roller)
                else if (s1.Type == SupportType.Roller && s2.Type == SupportType.Roller)
                {
                    if (Math.Abs(totalExternalFx) > 0.01f)
                    {
                        results.Message = "Схема с двумя подвижными опорами неустойчива при горизонтальных нагрузках.";
                        results.IsStaticallyDeterminate = false; // Или можно сказать isUnstable = true
                        return results;
                    }
                    r1.Rx = 0; r2.Rx = 0; // Нет горизонтальных реакций

                    // ΣM_s1 = 0 => R2y * (x_s2 - x_s1) + Моменты_от_сил_относительно_s1 = 0
                    float arm_s2 = s2.Location.X - s1.Location.X;
                    if (Math.Abs(arm_s2) < 0.01f)
                    {
                        results.Message = "Две подвижные опоры на одной вертикали. Система некорректна.";
                        results.IsStaticallyDeterminate = false; return results;
                    }
                    float sumMomentsAboutS1 = 0;
                    foreach (var force in forcesOnBeam)
                    {
                        float angleRad = force.AngleDegrees * (float)Math.PI / 180f;
                        sumMomentsAboutS1 += (force.Magnitude * (float)Math.Sin(angleRad)) * (force.Location.X - s1.Location.X);
                    }
                    r2.Ry = -sumMomentsAboutS1 / arm_s2;

                    // ΣFy = 0 => R1y + R2y + totalExternalFy = 0
                    r1.Ry = -totalExternalFy - r2.Ry;

                    results.SupportReactions.Add(r1);
                    results.SupportReactions.Add(r2);
                    results.IsSolved = true;
                }
                else
                {
                    results.Message = "Данная комбинация двух опор пока не поддерживается для автоматического расчета.";
                    results.IsStaticallyDeterminate = false;
                }
            }
            // Случай с одной опорой (например, заделка)
            else if (supportsOnBeam.Count == 1)
            {
                SupportElement s = supportsOnBeam[0];
                var r = new SupportReaction { Support = s };
                if (s.Type == SupportType.Fixed) // Жесткая заделка
                {
                    // 3 неизвестных: Rx, Ry, Mz
                    // 1. ΣFx = 0 => Rx + totalExternalFx = 0
                    r.Rx = -totalExternalFx;
                    // 2. ΣFy = 0 => Ry + totalExternalFy = 0
                    r.Ry = -totalExternalFy;
                    // 3. ΣM_support = 0 => Mz + Моменты_от_сил_относительно_опоры = 0
                    // sumMomentsAboutRefPoint уже посчитан относительно этой опоры (т.к. она первая и единственная)
                    r.Mz = -sumMomentsAboutRefPoint;

                    results.SupportReactions.Add(r);
                    results.IsSolved = true;
                }
                else
                {
                    results.Message = "Схема с одной опорой (не заделкой) является статически изменяемой (неустойчивой).";
                    results.IsStaticallyDeterminate = false;
                }
            }
            else // Больше двух опор или 0 (уже обработано)
            {
                results.Message = "Системы с более чем двумя опорами (или 0) требуют более сложного анализа (возможно, статически неопределимы).";
                results.IsStaticallyDeterminate = false;
            }

            return results;
        }

        private bool IsForceNearBeam(ForceElement force, BeamElement beam)
        {
            float beamY = beam.StartPoint.Y; // Предполагаем горизонтальную балку для простоты
            float beamMinX = Math.Min(beam.StartPoint.X, beam.EndPoint.X);
            float beamMaxX = Math.Max(beam.StartPoint.X, beam.EndPoint.X);
            float toleranceY = 20f; // Допуск по Y
            float toleranceX = 5f;  // Небольшой допуск по X за пределы балки

            return force.Location.Y > beamY - toleranceY && force.Location.Y < beamY + toleranceY &&
                   force.Location.X > beamMinX - toleranceX && force.Location.X < beamMaxX + toleranceX;
        }
    }
}