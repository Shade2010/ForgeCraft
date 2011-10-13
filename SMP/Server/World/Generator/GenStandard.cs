﻿/*
	Copyright 2011 ForgeCraft team
	
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
using System;
using LibNoise;

namespace SMP {

    /// <summary>
    /// Default terrain generator.
    /// </summary>
    public class GenStandard : ChunkGen {
        Random random;
        Perlin perlin;
        Perlin perlin2;
        Perlin desertSelector;
        Voronoi voronoi;
        RidgedMultifractal caves;
        RidgedMultifractal mountains;
        Perlin mountains2;

        public GenStandard(int seed) {
            random = new Random(seed);

            perlin = new Perlin();
            perlin.Frequency = 0.009;
            perlin.Persistence = 0.3;
            perlin.Seed = seed;

            perlin2 = new Perlin();
            perlin2.Frequency = 0.007;
            perlin2.Persistence = 0.6;
            perlin2.Lacunarity = 0.1;
            perlin2.Seed = perlin.Seed - 7;

            caves = new RidgedMultifractal();
            caves.Frequency = 0.0089;
            caves.Seed = perlin2.Seed + 99;

            voronoi = new Voronoi();
            voronoi.Frequency = 0.008;
            voronoi.Seed = perlin.Seed + 2;

            desertSelector = new Perlin();
            desertSelector.Frequency = 0.01;
            desertSelector.Persistence = 0.4;
            desertSelector.Lacunarity = 0.14;
            desertSelector.Seed = perlin.Seed + 245;

            mountains = new RidgedMultifractal();
            mountains.Frequency = 0.021;
            mountains.NoiseQuality = NoiseQuality.Low;
            mountains.Seed = perlin.Seed + 41;

            mountains2 = new Perlin();
            mountains2.Frequency = 0.025;
            mountains2.NoiseQuality = NoiseQuality.Low;
            mountains2.Seed = perlin.Seed + 41;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="c"></param>
        public override void Generate(World w, Chunk c)
        {
            int cx = c.x << 4, cz = c.z << 4;
            int waterLevel = 64 + 15 / 2 - 4;
            if (w.seed == 0)
            {
                for (int x = 0; x < 16; ++x)
                    for (int z = 0; z < 16; ++z)
                    {
                        for (int y = 0; y < 1; ++y)
                            c.UNCHECKEDPlaceBlock(x, y, z, 0x07);
                        for (int y = 1; y < 50; ++y)
                            c.UNCHECKEDPlaceBlock(x, y, z, 0x01);
                        for (int y = 50; y < 65; ++y)
                            c.UNCHECKEDPlaceBlock(x, y, z, 0x03);
                        c.UNCHECKEDPlaceBlock(x, 65, z, 0x02);
                    }
            }
            else
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {

                        int v = (int)(64 + voronoi.GetValue(cx + x, 5, cz + z) * 7 + (perlin2.GetValue(cx + x, 7, cz + z) + (perlin.GetValue(cx + x, 10, cz + z))) * 15);

                        // Bedrock
                        int bedrockHeight = random.Next(1, 6);
                        for (int y = 0; y < bedrockHeight; y++)
							c.UNCHECKEDPlaceBlock(x, y, z, 0x07);

                        // Stone
                        for (int y = bedrockHeight; y < v - 5; y++)
							c.UNCHECKEDPlaceBlock(x, y, z, 0x01);

                        // Dirt
                        for (int y = v - 5; y < v; y++)
							c.UNCHECKEDPlaceBlock(x, y, z, (byte)(desertSelector.GetValue(cx + x, y, cz + z) > 0.4 ? 0x0C : 0x03));


                        if (v <= waterLevel)
                        {
							c.UNCHECKEDPlaceBlock(x, v, z, 0x0C); // Send
                        }
                        else
                        {
							c.UNCHECKEDPlaceBlock(x, v, z, (byte)(desertSelector.GetValue(cx + x, v, cz + z) > 0.35 ? 0x0C : 0x02));
                        }
                    }
                }

                for (int x = 0; x < 16; x++)
                {
                    //for( int y = 0; y < 128; y++ ) {
                    for (int z = 0; z < 16; z++)
                    {
                        var d = 0.0;
                        var prevV = (64 + voronoi.GetValue(cx + x, 5, cz + z) * 7 + (perlin2.GetValue(cx + x, 7, cz + z) + (perlin.GetValue(cx + x, 10, cz + z))) * 15);
                        if (prevV < waterLevel)
                            d = (waterLevel - prevV) * 0.78;

                        var v = (-mountains.GetValue(cx + x, 127, cz + z));
                        if (v >= d)
                        {
                            var h = (64 + (int)(v * 39));

                            int lvl = 0;

                            for (int y = h - 1; y >= 0; y--)
                            {
                                var mv = mountains2.GetValue(cx + x, y, cz + z);
                                bool desert = desertSelector.GetValue(cx + x, y, cz + z) > 0.4;
                                //if( mv > 0.4 ) {
                                if (lvl == 0)
									c.UNCHECKEDPlaceBlock(x, y, z, (byte)(desert ? 0x0C : 0x02));
                                else if (lvl < 5)
									c.UNCHECKEDPlaceBlock(x, y, z, (byte)(desert ? 0x0C : 0x03));
                                else
									c.UNCHECKEDPlaceBlock(x, y, z, 0x01);

                                ++lvl;
                                //}
                            }
                        }
                    }
                    //}
                }


                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int y = 0; y < 128; y++)
                        {
                            if (caves.GetValue(cx + x, y, cz + z) > (128 - y) * 0.0132)
                            {
								c.UNCHECKEDPlaceBlock(x, y, z, 0x00);
                                if (c.SGB(x, y - 1, z) == 0x03)
									c.UNCHECKEDPlaceBlock(x, y - 1, z, 0x02);
                            }

                            if (y <= waterLevel && c.SGB(x, y, z) == 0x00)
                            {
								c.UNCHECKEDPlaceBlock(x, y, z, 0x08);
                            }
                        }
                    }
                }
            }
        }

        public override void SetSeed(int seed)
        {
            random = new Random(seed);
            perlin.Seed = seed;
            perlin2.Seed = seed - 7;
            caves.Seed = seed + 99;
            voronoi.Seed = seed + 2;
            desertSelector.Seed = seed + 245;
            mountains.Seed = seed + 41;
            mountains2.Seed = seed + 41;
        }
    }
}
