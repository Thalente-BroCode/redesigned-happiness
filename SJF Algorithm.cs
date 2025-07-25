using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Short_Job_First_Algorithm
{
    class Program
    {
        static int TotalMemory = 100;
        static List<MemoryBlock> memory = new List<MemoryBlock> {
        new MemoryBlock { Start = 0, Size = TotalMemory, IsFree = true, ProcessId = null }
    };
        static void Main(string[] args)
        {
            List<Process> processList = new List<Process>();
            Console.Write("Enter number of processes: ");
            int n = int.Parse(Console.ReadLine());

            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                Process p = new Process
                {
                    Id = i + 1,
                    BurstTime = rand.Next(2, 10),
                    MemoryRequired = rand.Next(10, 30),
                    State = "Ready"
                };
                p.RemainingTime = p.BurstTime;
                processList.Add(p);
            }

            Console.WriteLine("\nStarting simulation...\n");

            // Sort processes by BurstTime (SJF)
            var readyQueue = processList.OrderBy(p => p.BurstTime).ToList();

            while (readyQueue.Any())
            {
                var currentProcess = readyQueue.First();

                if (AllocateMemory(currentProcess))
                {
                    currentProcess.State = "Running";
                    DisplayMemory();
                    Console.WriteLine($"P{currentProcess.Id} is running for {currentProcess.BurstTime} time units...");
                    Thread.Sleep(1000);

                    // Simulate interrupt randomly
                    if (rand.NextDouble() < 0.3)
                    {
                        Console.WriteLine($"[INTERRUPT] P{currentProcess.Id} interrupted. Moving to waiting state.");
                        currentProcess.State = "Waiting";
                        Thread.Sleep(500);
                        currentProcess.State = "Ready";
                        continue; // Go back to queue without completing
                    }

                    currentProcess.State = "Terminated";
                    Console.WriteLine($"P{currentProcess.Id} completed.\n");
                    DeallocateMemory(currentProcess.Id);
                    readyQueue.Remove(currentProcess);
                    CompactMemory();
                }
                else
                {
                    Console.WriteLine($"Not enough memory for P{currentProcess.Id}. Skipping for now.\n");
                    // Move process to end of queue to simulate wait
                    readyQueue.Remove(currentProcess);
                    readyQueue.Add(currentProcess);
                }

                Thread.Sleep(500);
            }

            Console.WriteLine("\nAll processes completed. Final memory state:");
            DisplayMemory();
        }

        static bool AllocateMemory(Process p)
        {
            for (int i = 0; i < memory.Count; i++)
            {
                var block = memory[i];
                if (block.IsFree && block.Size >= p.MemoryRequired)
                {
                    int leftover = block.Size - p.MemoryRequired;

                    // Update current block
                    block.Size = p.MemoryRequired;
                    block.IsFree = false;
                    block.ProcessId = p.Id;

                    if (leftover > 0)
                    {
                        memory.Insert(i + 1, new MemoryBlock
                        {
                            Start = block.Start + p.MemoryRequired,
                            Size = leftover,
                            IsFree = true,
                            ProcessId = null
                        });
                    }

                    return true;
                }
            }
            return false;
        }

        static void DeallocateMemory(int processId)
        {
            foreach (var block in memory)
            {
                if (block.ProcessId == processId)
                {
                    block.IsFree = true;
                    block.ProcessId = null;
                }
            }
        }

        static void CompactMemory()
        {
            Console.WriteLine("Compacting memory...");
            var newMemory = new List<MemoryBlock>();
            int currentStart = 0;

            foreach (var block in memory.Where(b => !b.IsFree))
            {
                newMemory.Add(new MemoryBlock
                {
                    Start = currentStart,
                    Size = block.Size,
                    IsFree = false,
                    ProcessId = block.ProcessId
                });
                currentStart += block.Size;
            }

            int freeMemory = TotalMemory - currentStart;
            if (freeMemory > 0)
            {
                newMemory.Add(new MemoryBlock
                {
                    Start = currentStart,
                    Size = freeMemory,
                    IsFree = true,
                    ProcessId = null
                });
            }

            memory = newMemory;
            DisplayMemory();
        }

        static void DisplayMemory()
        {
            Console.WriteLine("Memory Map:");
            foreach (var block in memory)
            {
                string status = block.IsFree ? "Free" : $"P{block.ProcessId}";
                Console.WriteLine($"[{block.Start}-{block.Start + block.Size - 1}] : {status}");
            }
            Console.WriteLine();
        }
    }
}
