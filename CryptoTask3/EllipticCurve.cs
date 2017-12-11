using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTask3
{
    public static class EllipticCurve
    {
        private const int M = 67;
        private const int a = 0;
        private const int b = 1;
        private const int q = 241;

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        public static List<Point> Points { get; private set; } = new List<Point>();

        public static void GeneratePoints()
        {
            for (int x = 0; x < M; x++)
            {
                var y2 = ((int) (Math.Pow(x, 3) + a * x + b))%M;

                for (int y = 0; y < M; y++)
                {
                    if ((y*y)%M == y2)
                    {
                        Points.Add(new Point(x, y));
                    }
                }
            }
        }

        public static int Generate_nA()
        {
            lock (syncLock)
            {
                return random.Next(0, M);
            }
        }

        public static Point GeneratePublicKey(int nA, Point point)
        {
            Point p = point;

            for (int i = 1; i < nA; i++)
            {
                p = Add(p, point);
            }
            
            return p;
        }

        public static Point Sign(string m, Point G, int nA)
        {
            byte[] hm = ComputeHash(Encoding.ASCII.GetBytes(m));
            int r;
            int s;

            do
            {
                int k;
                do
                {
                    k = Generate_k();
                    Point p = GeneratePublicKey(k, G);
                    r = p.X%q;

                } while (r == 0);

                s = (Inverse(k, q)*(BitConverter.ToInt32(hm, 0) + nA * r))%q;
                if (s < 0)
                {
                    s += q;
                }

            } while (s == 0);

            //Console.WriteLine("k = {0}", k);

            return new Point(r, s);
        }

        public static bool IsSignCorrect(string m, int r, int s, Point pA, Point G)
        {
            byte[] hm = ComputeHash(Encoding.ASCII.GetBytes(m));

            if ((r >= 1 && r < q - 1) && ((s >= 1 && s < q - 1)))
            {
                int w = Inverse(s, q);
                if (w < 0)
                {
                    w += q;
                }

                int u1 = (BitConverter.ToInt32(hm, 0)*w)%q;
                if (u1 < 0)
                {
                    u1 += q;
                }

                int u2 = (r*w)%q;

                Point p1 = GeneratePublicKey(u1, G);
                Point p2 = GeneratePublicKey(u2, pA);
                Point p = Add(p1, p2);

                int r_ptr = p.X%q;

                return r_ptr == r;
            }

            return false;
        }

        public static int Generate_k()
        {
            lock (syncLock)
            {
                return random.Next(1, q - 1);
            }
        }

        private static Point Add(Point p1, Point p2)
        {
            Point p3 = new Point();

            int dy;
            int dx;

            if (p1 == p2)
            {
                dy = 3*p1.X*p1.X + a;
                dx = 2*p1.Y;
            }
            else
            {
                dy = p2.Y - p1.Y;
                dx = p2.X - p1.X;
            }

            if (dx < 0)
            {
                dx += M;
            }
            if (dy < 0)
            {
                dy += M;
            }

            int lambda = (dy * Inverse(dx, M)) % M;
            if (lambda < 0)
            {
                lambda += M;
            }

            p3.X = (lambda * lambda - p1.X - p2.X) % M;
            p3.Y = (lambda * (p1.X - p3.X) - p1.Y) % M;

            if (p3.X < 0)
            {
                p3.X += M;
            }
            if (p3.Y < 0)
            {
                p3.Y += M;
            }

            return p3;
        }

        /// <summary>
        /// The inverse element of a modulo b 
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <returns>
        /// Returns the inverse of a modulo b if it exists
        /// 0 otherwise
        /// </returns>
        private static int Inverse(int a, int b)
        {
            int x;
            int y;

            int d = Euclid(a, b, out x, out y);

            if (d == 1)
            {
                return (x%b + b)%b;
            }

            return 0;
        }

        /// <summary>
        /// Сalculates a * x + b * y = gcd(a, b)
        /// </summary>
        /// <param name="a">a</param>
        /// <param name="b">b</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        private static int Euclid(int a, int b, out int x, out int y)
        {
            if (a == 0)
            {
                x = 0;
                y = 1;

                return b;
            }

            int x1;
            int y1;
            int d = Euclid(b % a, a, out x1, out y1);
            x = y1 - (b / a) * x1;
            y = x1;

            return d;
        }

        private static byte[] ComputeHash(byte[] data)
        {
            using (var alg = SHA1.Create())
            {
                return alg.ComputeHash(data);
            }
        }
    }
}