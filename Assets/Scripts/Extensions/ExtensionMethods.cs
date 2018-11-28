using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyExtensions
{
    public static class Vector3Extension
    {
        /// <summary>
        /// Отразить вектор от нормали поверхности под углом относительно нормали
        /// </summary>
        /// <param name="toReflect">Отражаемый вектор</param>
        /// <param name="normal">Нормаль поверхности</param>
        /// <param name="angle">Угол отражения(v)</param>
        /// <returns></returns>
        public static Vector3 ReflectAngle(Vector3 toReflect, Vector3 normal, float angleH)
        {
            float magnitude = toReflect.magnitude;
            Vector3 hor = magnitude * Mathf.Cos(angleH) * (normal + toReflect).normalized;
            Vector3 ver = magnitude * Mathf.Sin(angleH) * normal.normalized;
            return Vector3.ClampMagnitude( hor + ver, magnitude);
        }
        /// <summary>
        /// Отразить вектор от нормали поверхности под углом к нормали и к вектору, перп. к нормали и направленным в ту же сторону что и исх. вектор 
        /// </summary>
        /// <param name="toReflect">Отражаемый вектор</param>
        /// <param name="normal">Нормаль поверхности</param>
        /// <param name="angle">Угол отражения(v)</param>
        /// <param name="randAngle">Угол отражения(h)</param>
        /// <returns></returns>
        public static Vector3 RandomReflectAngle(Vector3 toReflect, Vector3 normal, float angleV,float angleH)
        {
            float magnitude = toReflect.magnitude;
            Vector3 hor = magnitude * Mathf.Cos(angleV) * (normal + toReflect).normalized;
            Vector3 ver = magnitude * Mathf.Sin(angleV) * normal.normalized;
            Vector3 perp = Vector3.Cross(normal, toReflect).normalized * Mathf.Cos(angleH) * magnitude;
            return Vector3.ClampMagnitude(perp + hor + ver, magnitude) ;

        }
    }
}


