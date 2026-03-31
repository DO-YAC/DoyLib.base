using System;
using System.Net;
using System.Threading;
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
    private Candle[] mRawWindow = null!;
    private int mMaxSize;
    private int mWindowCount;
    private int mRawWindowStart;

    private DateTime? LatestTimestamp
    {
        get
        {
            if (mWindowCount == 0) return null;

            if (mWindowCount < mMaxSize)
                return mRawWindow[mWindowCount - 1].Timestamp;

            return mRawWindow[(mRawWindowStart - 1 + mMaxSize) % mMaxSize].Timestamp;
        }
    }

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
        mRawWindow = new Candle[maxSize];
        mWindowCount = 0;
        mRawWindowStart = 0;
        logger.LogInformation("CandleWindowService initialized with maxSize: {MaxSize}", maxSize);
    }
    
    private void UpdateLatestCandle(Candle candle)
    {
        if (mWindowCount == 0)
        {
            return;
        }

        if (mWindowCount < mMaxSize)
        {
            mRawWindow[mWindowCount - 1] = candle.Clone();
        }
        else
        {
            mRawWindow[(mRawWindowStart - 1 + mMaxSize) % mMaxSize] = candle.Clone();
        }
    }

    public void AddCandle(Candle[]? candles)
    {
        if (candles == null)
        {
            var typedEx = new DoyVestmentException(
                "Argument cannot be null",
                HttpStatusCode.BadRequest,
                ExceptionSeverityLevel.Inoperable);

            exceptionHandler.HandleException(typedEx, logger);

            return;
        }
        
        foreach (var candle in candles)
        {
            AddCandle(candle);
        }
    }

    public void AddCandle(Candle? candle)
    {
        if (candle == null)
        {
            var typedEx = new DoyVestmentException(
                "Argument cannot be null",
                HttpStatusCode.BadRequest,
                ExceptionSeverityLevel.Inoperable);

            exceptionHandler.HandleException(typedEx, logger);

            return;
        }

        lock (mLock)
        {
            if (LatestTimestamp == candle.Timestamp)
            {
                UpdateLatestCandle(candle);
                return;
            }

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
