using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace AsyncReaderWriterLockTest
{
    internal class Program
    {
        private static AsyncSemaphore consoleLock;
        private static AsyncReaderWriterLock locker;
        private static int readers;
        private static int upgradeables;
        private static int writers;
        public static async Task RunAsync(int id)
        {
            Console.WriteLine($"Run {id}");
            await Task.Delay(1000);
            for (var i = 0; i < id * 2; i++)
                using (await locker.ReadLockAsync())
                {
                    using (await consoleLock.EnterAsync())
                    {
                        Interlocked.Increment(ref readers);
                        Console.WriteLine($"Begin Read Loop {i + 1} Id {id} number of readers {readers}");
                    }

                    await Task.Delay(1000);
                    using (await consoleLock.EnterAsync())
                    {
                        Interlocked.Decrement(ref readers);
                        Console.WriteLine($"End Read Loop {i + 1} Id {id} number of readers {readers}");
                    }
                }

            using (await locker.UpgradeableReadLockAsync())
            {
                using (await consoleLock.EnterAsync())
                {
                    Interlocked.Increment(ref upgradeables);
                    Console.WriteLine($"Begin Upgradeable Id {id} number of upgradeables {upgradeables}");
                }

                await Task.Delay(1000);
                using (await consoleLock.EnterAsync())
                {
                    Console.WriteLine($"Wait Upgarde Id {id} number of upgradeables {upgradeables}");
                }

                using (await locker.WriteLockAsync())
                {
                    using (await consoleLock.EnterAsync())
                    {
                        Interlocked.Increment(ref writers);
                        Console.WriteLine($"Begin Write Id {id} number of writers {writers}");
                    }

                    await Task.Delay(1000);
                    using (await consoleLock.EnterAsync())
                    {
                        Interlocked.Decrement(ref writers);
                        Console.WriteLine($"End Write ID {id} number of writers {writers}");
                    }
                }

                using (await consoleLock.EnterAsync())
                {
                    Interlocked.Decrement(ref upgradeables);
                    Console.WriteLine($"End Upgradeable Id {id} number of upgradeables {upgradeables}");
                }
            }
        }

        private static async Task Main(string[] args)
        {
            locker = new AsyncReaderWriterLock();
            consoleLock = new AsyncSemaphore(1);
            Console.WriteLine("Hello World!");
            var t1 = RunAsync(1);
            var t2 = RunAsync(2);
            var t3 = RunAsync(3);
            var t4 = RunAsync(4);
            var t5 = RunAsync(5);
            Task.WaitAll(t1, t2, t3, t4, t5);
            Thread.Sleep(5000);
        }
    }
}