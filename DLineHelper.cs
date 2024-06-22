using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace D3DWinUI3;

public class DLineHelper
{
    public static void SimpleMovingAverageInPlace(ObservableCollection<DLinePoint> points, int windowSize)
    {
        List<DLinePoint> originalPoints = new List<DLinePoint>(points);

        for (int i = 0; i < points.Count; i++)
        {
            float sumX = 0, sumY = 0, sumPressure = 0;
            int count = 0;

            for (int j = i - windowSize; j <= i + windowSize; j++)
            {
                if (j >= 0 && j < originalPoints.Count)
                {
                    sumX += originalPoints[j].X;
                    sumY += originalPoints[j].Y;
                    sumPressure += originalPoints[j].Pressure;
                    count++;
                }
            }

            points[i] = new DLinePoint(sumX / count, sumY / count, sumPressure / count);
        }
    }
    

    public static void SplineInterpolationInPlace(ObservableCollection<DLinePoint> points, int resolution = 100)
    {
        int n = points.Count;
        if (n < 2)
        {
            return; // 插值需要至少两个点
        }

        double[] xs = new double[n];
        double[] ys = new double[n];
        double[] pressures = new double[n];

        for (int i = 0; i < n; i++)
        {
            xs[i] = points[i].X;
            ys[i] = points[i].Y;
            pressures[i] = points[i].Pressure;
        }

        var splineX = CubicSpline.InterpolateNaturalSorted(xs, xs);
        var splineY = CubicSpline.InterpolateNaturalSorted(xs, ys);
        var splinePressure = CubicSpline.InterpolateNaturalSorted(xs, pressures);

        // 计算新的插值点
        for (int i = 0; i < n; i++)
        {
            float newX = (float)splineX.Interpolate(xs[i]);
            float newY = (float)splineY.Interpolate(xs[i]);
            float newPressure = (float)splinePressure.Interpolate(xs[i]);

            points[i] = new DLinePoint(newX, newY, 2);
        }
    }


    public static void GaussianSmoothingInPlace(ObservableCollection<DLinePoint> points, int windowSize, double sigma)
    {
        List<DLinePoint> originalPoints = new List<DLinePoint>(points);
        double[] weights = new double[2 * windowSize + 1];

        for (int i = -windowSize; i <= windowSize; i++)
        {
            weights[i + windowSize] = Math.Exp(-0.5 * (i * i) / (sigma * sigma));
        }

        for (int i = 0; i < points.Count; i++)
        {
            double sumX = 0, sumY = 0, sumPressure = 0, sumWeights = 0;

            for (int j = -windowSize; j <= windowSize; j++)
            {
                int index = i + j;
                if (index >= 0 && index < originalPoints.Count)
                {
                    double weight = weights[j + windowSize];
                    sumX += originalPoints[index].X * weight;
                    sumY += originalPoints[index].Y * weight;
                    sumPressure += originalPoints[index].Pressure * weight;
                    sumWeights += weight;
                }
            }

            points[i] = new DLinePoint((float)(sumX / sumWeights), (float)(sumY / sumWeights),
                (float)(sumPressure / sumWeights));
        }
    }
}