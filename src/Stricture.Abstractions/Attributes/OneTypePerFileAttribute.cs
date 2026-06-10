using System;

namespace Stricture
{
    /// <summary>Turns on the one-top-level-type-per-file rule.</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class OneTypePerFileAttribute : Attribute
    {
        /// <summary>
        /// When <see langword="true"/>, co-located types must share a common stem (e.g. <c>Foo</c> in
        /// <c>FooRequest</c>/<c>FooResponse</c>).
        /// </summary>
        public bool RequireSharedStem { get; set; } = true;
    }
}
