// Copyright (C) 2024, The Duplicati Team
// https://duplicati.com, hello@duplicati.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System.Diagnostics;
using Duplicati.StreamUtil;

namespace Tests;

public class MeasureTest
{
    [Test]
    public async Task MeasureThrottledStream()
    {
        var delta = 0.01;
        var source = new MemoryStream();
        source.SetLength(1024 * 1024 * 200); // 100 MB
        var target = new MemoryStream();

        var throttleManager = new ThrottleManager
        {
            Limit = 1024 * 1024 * 10 // 10 MB/s
        };
        var throttledStream = new ThrottleEnabledStream(source, throttleManager);
        var measureStream = new SpeedMeasuringStream(throttledStream);

        var targetTime = TimeSpan.FromSeconds(source.Length / throttleManager.Limit);

        var start = DateTime.Now;
        var copyTask = measureStream.CopyToAsync(target);
        await Task.Delay(targetTime / 2);
        var totalSpeed1 = measureStream.TotalBytesPerSecond;
        var totalWindow1 = measureStream.RecentBytesPerSecond;
        await copyTask;
        var elapsed = DateTime.Now - start;

        var totalSpeed2 = measureStream.TotalBytesPerSecond;
        var totalWindow2 = measureStream.RecentBytesPerSecond;

        Assert.That(Math.Abs(totalSpeed1 - throttleManager.Limit), Is.LessThan(throttleManager.Limit * delta));
        Assert.That(Math.Abs(totalSpeed2 - throttleManager.Limit), Is.LessThan(throttleManager.Limit * delta));
        Assert.That(Math.Abs(totalWindow1 - throttleManager.Limit), Is.LessThan(throttleManager.Limit * delta));
        Assert.That(Math.Abs(totalWindow2 - throttleManager.Limit), Is.LessThan(throttleManager.Limit * delta));
    }

}
