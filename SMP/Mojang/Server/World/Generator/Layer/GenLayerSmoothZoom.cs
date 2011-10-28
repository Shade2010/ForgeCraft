﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMP
{
    public class GenLayerSmoothZoom : GenLayer
    {
        public GenLayerSmoothZoom(long l, GenLayer genlayer)
            : base(l)
        {
            field_35023_a = genlayer;
        }

        public override int[] func_35018_a(int i, int j, int k, int l)
        {
            int i1 = i >> 1;
            int j1 = j >> 1;
            int k1 = (k >> 1) + 3;
            int l1 = (l >> 1) + 3;
            int[] ai = field_35023_a.func_35018_a(i1, j1, k1, l1);
            int[] ai1 = IntCache.func_35549_a(k1 * 2 * (l1 * 2));
            int i2 = k1 << 1;
            for(int j2 = 0; j2 < l1 - 1; j2++)
            {
                int k2 = j2 << 1;
                int i3 = k2 * i2;
                int j3 = ai[0 + (j2 + 0) * k1];
                int k3 = ai[0 + (j2 + 1) * k1];
                for(int l3 = 0; l3 < k1 - 1; l3++)
                {
                    func_35017_a(l3 + i1 << 1, j2 + j1 << 1);
                    int i4 = ai[l3 + 1 + (j2 + 0) * k1];
                    int j4 = ai[l3 + 1 + (j2 + 1) * k1];
                    ai1[i3] = j3;
                    ai1[i3++ + i2] = j3 + ((k3 - j3) * func_35016_a(256)) / 256;
                    ai1[i3] = j3 + ((i4 - j3) * func_35016_a(256)) / 256;
                    int k4 = j3 + ((i4 - j3) * func_35016_a(256)) / 256;
                    int l4 = k3 + ((j4 - k3) * func_35016_a(256)) / 256;
                    ai1[i3++ + i2] = k4 + ((l4 - k4) * func_35016_a(256)) / 256;
                    j3 = i4;
                    k3 = j4;
                }

            }

            int[] ai2 = IntCache.func_35549_a(k * l);
            for(int l2 = 0; l2 < l; l2++)
            {
                Array.Copy(ai1, (l2 + (j & 1)) * (k1 << 1) + (i & 1), ai2, l2 * k, k);
            }

            return ai2;
        }

        public static GenLayer func_35030_a(long l, GenLayer genlayer, int i)
        {
            object obj = genlayer;
            for (int j = 0; j < i; j++)
            {
                obj = new GenLayerSmoothZoom(l + (long)j, ((GenLayer)(obj)));
            }

            return ((GenLayer)(obj));
        }
    }
}
