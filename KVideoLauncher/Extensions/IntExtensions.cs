namespace KVideoLauncher.Extensions;

public static class IntExtensions
{
    public static int MathMod(this int a, int b) => (a % b + b) % b;
}