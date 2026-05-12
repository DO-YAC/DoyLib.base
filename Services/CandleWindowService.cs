using System;
using System.Net;
using System.Threading;
using doylib.Services.Interfaces;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using DoyVestment.Framework.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace doylib.Services;
internal class CandleWindowService(
    ILogger<CandleWindowService> logger,
    IDoyExceptionHandler exceptionHandler) : ICandleWindowService
{
    private readonly Lock mLock = new();
    private Candle[] mWindow = null!;
    private int mMaxSize;
    private int mCandleCount;

    private DateTime? LatestTimestamp => mCandleCount == 0 ? null : mWindow[mCandleCount - 1].Timestamp;

    public ReadOnlyMemory<Candle> Window => new ReadOnlyMemory<Candle>(mWindow, 0, mCandleCount);

    public void Initialize(int maxSize)
    {
        if (maxSize <= 0)
        {
            var typedEx = new DoyVestmentException(
                "Window size must be greater than 0.",
                HttpStatusCode.BadRequest,
                ExceptionSeverityLevel.Inoperable);

            exceptionHandler.HandleException(typedEx, logger);

            return;
        }

        mMaxSize = maxSize;
        mWindow = new Candle[maxSize];
        mCandleCount = 0;
        logger.LogInformation("CandleWindowService initialized with maxSize: {MaxSize}", maxSize);
    }

    public void AddCandle(Candle[] candles)
    {
        foreach (var candle in candles)
        {
            AddCandle(candle);
        }
    }

    public void AddCandle(Candle candle)
    {
        lock (mLock)
        {
            if (LatestTimestamp == candle.Timestamp)
            {
                mWindow[mCandleCount - 1] = candle;
                return;
            }

            if (mCandleCount < mMaxSize)
            {
                mWindow[mCandleCount] = candle;
                mCandleCount++;
            }
            else
            {
                Array.Copy(mWindow, 1, mWindow, 0, mMaxSize - 1);
                mWindow[mMaxSize - 1] = candle;
            }

            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Added candle: {Symbol} {Timeframe} at {Timestamp}",
                        candle.Symbol, candle.Timeframe, candle.Timestamp);
            }
        }
    }
}
