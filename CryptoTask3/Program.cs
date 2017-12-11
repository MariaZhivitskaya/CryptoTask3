using System;
using System.IO;
using System.Linq;

namespace CryptoTask3
{
    class Program
    {
        static void Main(string[] args)
        {
            EllipticCurve.GeneratePoints();

            using (StreamWriter streamWriter = new StreamWriter("Points.txt"))
            {
                foreach (var point in EllipticCurve.Points)
                {
                    streamWriter.WriteLine(point.ToString());
                }
            }

            Point G;
            int nA1;
            Point pA;

            using (StreamWriter streamWriter = new StreamWriter("KeyExchange.txt"))
            {
                Random rand = new Random();

                G = EllipticCurve.Points.ElementAt(rand.Next(0, EllipticCurve.Points.Count));
                streamWriter.WriteLine("G = {0}", G.ToString());

                nA1 = EllipticCurve.Generate_nA();
                streamWriter.WriteLine("nA1 = {0}", nA1);

                int nA2 = EllipticCurve.Generate_nA();
                streamWriter.WriteLine("nA2 = {0}", nA2);

                pA = EllipticCurve.GeneratePublicKey(nA1, G);
                streamWriter.WriteLine("pA = {0}", pA.ToString());

                Point pB = EllipticCurve.GeneratePublicKey(nA2, G);
                streamWriter.WriteLine("pB = {0}", pB.ToString());

                Point K1 = EllipticCurve.GeneratePublicKey(nA1, pB);
                streamWriter.WriteLine("K = nApB = {0}", K1.ToString());

                Point K2 = EllipticCurve.GeneratePublicKey(nA2, pA);
                streamWriter.WriteLine("K = nBpA = {0}", K2.ToString());
            }

            string m = "CauseI'mthelightingtoyourstorm";

            using (StreamWriter streamWriter = new StreamWriter("ECDSA.txt"))
            {
                Point p = EllipticCurve.Sign(m, G, nA1);
                streamWriter.WriteLine("User A: Sign = {0}", p.ToString());

                bool res = EllipticCurve.IsSignCorrect(m, p.X, p.Y, pA, G);

                if (res)
                {
                    streamWriter.WriteLine("User B: sign is correct");
                }
                else
                {
                    streamWriter.WriteLine("User B: sign is not correct");
                }
            }
        }
    }
}
