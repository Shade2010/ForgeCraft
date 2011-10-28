﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMP
{
    public class Chunker
    {
        private Queue<ChunkGenQueue> genQueue;
        private List<ChunkLoadQueue> loadQueue;
        private List<ChunkSendQueue> sendQueue;
        private List<ChunkGenQueue> generated;
        private Thread genThread, loadThread, sendThread;


        public Chunker()
        {
            genQueue = new Queue<ChunkGenQueue>();
            sendQueue = new List<ChunkSendQueue>();
            loadQueue = new List<ChunkLoadQueue>();
            generated = new List<ChunkGenQueue>();
        }

        public void Start()
        {
            genThread = new Thread(new ThreadStart(delegate
            {
                Parallel.For(0, Server.genThreads, delegate(int wtf) // The int is so the compiler will shut up.
                {
                    ChunkGenQueue cgq; /*Chunk ch;*/ Point pt;
                    while (!Server.s.shuttingDown)
                    {
                        try
                        {
                            if (genQueue.Count < 1) { Thread.Sleep(100); continue; }
                            lock (genQueue)
                                cgq = genQueue.Dequeue();
                            lock (generated)
                                generated.Add(cgq);
                            /*ch =*/ cgq.world.GenerateChunk(cgq.x, cgq.z);
                            pt = new Point(cgq.x, cgq.z);
                            /*lock (cgq.world.chunkData)
                                if (!cgq.world.chunkData.ContainsKey(pt))
                                    cgq.world.chunkData.Add(pt, ch);*/
                            if (cgq.world.chunkData.ContainsKey(pt))
                            {
                                cgq.world.chunkData[pt].PostGenerate(cgq.world);
                                cgq.world.chunkData[pt].PostLoad(cgq.world);
                            }
                            lock (generated)
                                generated.Remove(cgq);
                        }
                        catch { }
                    }
                });
            }));
            genThread.Start();

            loadThread = new Thread(new ThreadStart(delegate
            {
                while (!Server.s.shuttingDown)
                {
                    try
                    {
                        if (loadQueue.Count < 1) { Thread.Sleep(100); continue; }
                        loadQueue.ForEach(delegate(ChunkLoadQueue clq)
                        {
                            if (clq.unload)
                            {
                                if (clq.world.chunkData.ContainsKey(new Point(clq.x, clq.z)))
                                    clq.world.UnloadChunk(clq.x, clq.z);
                            }
                            else
                            {
                                if (!clq.world.chunkData.ContainsKey(new Point(clq.x, clq.z)))
                                    clq.world.LoadChunk(clq.x, clq.z, true, false);
                            }
                            Thread.Sleep(5);
                        });
                        loadQueue.Clear();
                    }
                    catch { }
                }
            }));
            loadThread.Start();

            sendThread = new Thread(new ThreadStart(delegate
            {
                Point pt;
                while (!Server.s.shuttingDown)
                {
                    try
                    {
                        if (sendQueue.Count < 1) { Thread.Sleep(100); continue; }
                        sendQueue.ForEach(delegate(ChunkSendQueue csq)
                        {
                            pt = new Point(csq.x, csq.z);
                            if (csq.player.level.chunkData.ContainsKey(pt) && csq.player.level.chunkData[pt].generated)
                            {
                                //Console.WriteLine("SENT " + csq.x + "," + csq.z);
                                csq.player.SendChunk(csq.player.level.chunkData[pt]);
                                csq.player.level.chunkData[pt].Update(csq.player.level, csq.player);
                                csq.sent = true;
                            }
                            if ((DateTime.Now - csq.time).TotalSeconds >= 60 && sendQueue.Contains(csq))
                                csq.sent = true;
                            Thread.Sleep(5);
                        });
                        sendQueue.RemoveAll(csq => csq.sent);
                    }
                    catch { }
                }
            }));
            sendThread.Start();
        }

        public void QueueChunk(Point point, World world)
        {
            QueueChunk(point.x, point.z, world);
        }
        public void QueueChunk(int x, int z, World world)
        {
            ChunkGenQueue cgq = new ChunkGenQueue(x, z, world);
            lock (generated)
                if (generated.Contains(cgq)) return;
            lock (genQueue)
                genQueue.Enqueue(cgq);
        }

        public void QueueChunkLoad(Point point, bool unload, World world)
        {
            QueueChunkLoad(point.x, point.z, unload, world);
        }
        public void QueueChunkLoad(int x, int z, bool unload, World world)
        {
            ChunkLoadQueue clq = new ChunkLoadQueue(x, z, unload, world);
            loadQueue.RemoveAll(CLQ => (CLQ.x == x && CLQ.z == z && CLQ.world == world));
            loadQueue.Add(clq);
        }

        public void QueueChunkSend(Point point, Player player)
        {
            QueueChunkSend(point.x, point.z, player);
        }
        public void QueueChunkSend(int x, int z, Player player)
        {
            ChunkSendQueue csq = new ChunkSendQueue(x, z, player);
            if (sendQueue.Contains(csq)) return;
            sendQueue.Add(csq);
        }
    }


    public struct ChunkGenQueue
    {
        public int x, z;
        public World world;

        public ChunkGenQueue(Point point, World world) : this(point.x, point.z, world) { }
        public ChunkGenQueue(int x, int z, World world)
        {
            this.x = x;
            this.z = z;
            this.world = world;
        }
    }

    public struct ChunkLoadQueue
    {
        public bool unload;
        public int x, z;
        public World world;

        public ChunkLoadQueue(Point point, bool unload, World world) : this(point.x, point.z, unload, world) { }
        public ChunkLoadQueue(int x, int z, bool unload, World world)
        {
            this.x = x;
            this.z = z;
            this.unload = unload;
            this.world = world;
        }
    }

    public class ChunkSendQueue
    {
        public bool sent;
        public int x, z;
        public Player player;
        public DateTime time;

        public ChunkSendQueue(Point point, Player player) : this(point.x, point.z, player) { }
        public ChunkSendQueue(int x, int z, Player player)
        {
            this.x = x;
            this.z = z;
            this.player = player;
            this.time = DateTime.Now;
            this.sent = false;
        }
    }
}
