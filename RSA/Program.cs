using System;
using System.Numerics;

public class Program
{
    public static void Main()
    {
        Console.Write("Введите верхнюю границу для простых чисел p и q (минимум 10): ");
        long limit = long.Parse(Console.ReadLine()!);
        if (limit < 10) limit = 10;

        long p = GeneratePrime(limit);
        long q;

        do
        {
            q = GeneratePrime(limit);
        } while (q == p);

        Console.WriteLine($"Сгенерированное p = {p}");
        Console.WriteLine($"Сгенерированное q = {q}");

        BigInteger n = (BigInteger)p * q;
        BigInteger phi = (BigInteger)(p - 1) * (q - 1);

        BigInteger e = ChoosePublicExponent(phi);

        BigInteger d = ModInverse(e, phi);

        Console.WriteLine($"\nОткрытый ключ (e, n): ({e}, {n})");
        Console.WriteLine($"Закрытый ключ (d, n): ({d}, {n})");

        Console.Write($"\nВведите числовое сообщение m (от 0 до {n - 1}): ");
        BigInteger message = BigInteger.Parse(Console.ReadLine()!);

        if (message < 0 || message >= n)
        {
            Console.WriteLine($"Ошибка: сообщение должно быть меньше модуля n ({n}).");
            return;
        }

        BigInteger encrypted = FastPower(message, e, n);
        BigInteger decrypted = FastPower(encrypted, d, n);

        Console.WriteLine($"Зашифрованное сообщение c = {encrypted}");
        Console.WriteLine($"Расшифрованное сообщение m = {decrypted}");
        Console.WriteLine($"Успешно расшифровано: {message == decrypted}");
    }
    
    public static BigInteger FastPower(BigInteger baseValue, BigInteger exponent, BigInteger module)
    {
        BigInteger result = 1;
        baseValue %= module;

        while (exponent > 0)
        {
            if ((exponent & 1) == 1)
            {
                result = (result * baseValue) % module;
            }

            baseValue = (baseValue * baseValue) % module;
            exponent >>= 1;
        }

        return result;
    }

    public static bool IsPrimeNumber(long number, int round)
    {
        if (number < 2) return false;
        if (number == 2 || number == 3) return true;
        if (number % 2 == 0) return false;

        long nMinusOne = number - 1;
        int s = 0;
        long d = nMinusOne;

        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        for (int i = 0; i < round; i++)
        {
            long a = Random.Shared.NextInt64(2, nMinusOne);
            long x = (long)FastPower(a, d, number);

            if (x == 1 || x == nMinusOne)
                continue;

            bool witnessFound = false;
            for (int j = 0; j < s - 1; j++)
            {
                x = (long)FastPower(x, 2, number);

                if (x == 1)
                    return false;

                if (x == nMinusOne)
                {
                    witnessFound = true;
                    break;
                }
            }

            if (!witnessFound)
                return false;
        }

        return true;
    }

    public static long GeneratePrime(long limit)
    {
        long min = Math.Max(3, limit / 2);
        long start = Random.Shared.NextInt64(min, limit + 1);
        if (start % 2 == 0) start++;

        for (long i = start; i <= limit; i += 2)
        {
            if (IsPrimeNumber(i, 10)) return i;
        }

        for (long i = start - 2; i >= 3; i -= 2)
        {
            if (IsPrimeNumber(i, 10)) return i;
        }

        return 3;
    }

    public static BigInteger ChoosePublicExponent(BigInteger phi)
    {
        BigInteger standardE = 65537;
        if (standardE < phi && BigInteger.GreatestCommonDivisor(standardE, phi) == 1)
        {
            return standardE;
        }

        for (BigInteger e = 3; e < phi; e += 2)
        {
            if (BigInteger.GreatestCommonDivisor(e, phi) == 1)
            {
                return e;
            }
        }

        throw new InvalidOperationException("Не удалось подобрать открытую экспоненту.");
    }
    public static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m;
        BigInteger y = 0, x = 1;

        if (m == 1) return 0;

        while (a > 1)
        {
            BigInteger q = a / m;
            BigInteger t = m;

            m = a % m;
            a = t;
            t = y;

            y = x - q * y;
            x = t;
        }

        if (x < 0)
            x += m0;

        return x;
    }
}