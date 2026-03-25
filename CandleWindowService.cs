using System.Net;
using System.Threading;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using Microsoft.Extensions.Logging;

namespace doylib;
// IDoyExceptionHandler exceptionHandler gets added as soon as DOY-144 is merged
public class CandleWindowService(
    ILogger<CandleWindowService> logger) : ICandleWindowService
{
    private readonly Lock mLock = new();
    private Candle[] mRawWindow = null!;
    private int mMaxSize;
    private int mWindowCount;
    private int mRawWindowStart;

    public int Count => mWindowCount;

    public void Initialize(int maxSize)
    {
        if (maxSize <= 0)
        {
            var typedEx = new DoyVestmentException(
                "Window size must be greater than 0.",
                HttpStatusCode.BadRequest,
                ExceptionSeverityLevel.Inoperable);

            // exceptionHandler.HandleException(typedEx, logger);
        }

        mMaxSize = maxSize;
        mRawWindow = new Candle[maxSize];
        mWindowCount = 0;
        mRawWindowStart = 0;
        logger.LogInformation("TACandleWindowService initialized with maxSize: {MaxSize}", maxSize);
    }

    public void UpdateLatestCandle(Candle candle)
    {
        if (mWindowCount < mMaxSize)
        {
            mRawWindow[mWindowCount - 1] = candle.Clone();
        }
        else
        {
            mRawWindow[mRawWindowStart - 1] = candle.Clone();
        }
    }

    public void AddCandle(Candle candle)
    {
        if (candle == null)
        {
            var typedEx = new DoyVestmentException(
                "Argument cannot be null",
                HttpStatusCode.BadRequest,
                ExceptionSeverityLevel.Inoperable);

            // exceptionHandler.HandleException(typedEx, logger);

            return;
        }

        lock (mLock)
        {
            if (mWindowCount < mMaxSize)
            {
                mRawWindow[mWindowCount] = candle.Clone();
                mWindowCount++;
            }
            else
            {
                mRawWindow[mRawWindowStart] = candle.Clone();
                mRawWindowStart = (mRawWindowStart + 1) % mMaxSize;
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Added candle: {Symbol} {Timeframe} at {Timestamp}",
                        candle.Symbol, candle.Timeframe, candle.Timestamp); 
            }
        }
    }
}
