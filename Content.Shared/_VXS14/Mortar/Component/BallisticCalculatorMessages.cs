using System;
using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._VXS14.Mortar;

public static class MortarBallisticCalculatorEuiMsg
{
    [Serializable, NetSerializable]
    public sealed class Snapshot : EuiMessageBase
    {
        public Snapshot(float observerX, float observerY)
        {
            ObserverX = observerX;
            ObserverY = observerY;
        }

        public float ObserverX;
        public float ObserverY;
    }

    [Serializable, NetSerializable]
    public sealed class CalculateRequest : EuiMessageBase
    {
        public CalculateRequest(float mortarX, float mortarY, float deviationX, float deviationY)
        {
            MortarX = mortarX;
            MortarY = mortarY;
            DeviationX = deviationX;
            DeviationY = deviationY;
        }

        public float MortarX;
        public float MortarY;
        public float DeviationX;
        public float DeviationY;
    }

    [Serializable, NetSerializable]
    public sealed class Result : EuiMessageBase
    {
        public Result(float flightTimeSeconds, float expectedDeviation, float mortarToTargetDistance, float targetX, float targetY)
        {
            FlightTimeSeconds = flightTimeSeconds;
            ExpectedDeviation = expectedDeviation;
            MortarToTargetDistance = mortarToTargetDistance;
            TargetX = targetX;
            TargetY = targetY;
        }

        public float FlightTimeSeconds;
        public float ExpectedDeviation;
        public float MortarToTargetDistance;
        public float TargetX;
        public float TargetY;
    }

    [Serializable, NetSerializable]
    public sealed class Error : EuiMessageBase
    {
        public Error(string message)
        {
            Message = message;
        }

        public string Message;
    }
}
