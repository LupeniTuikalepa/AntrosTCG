using System.Collections.Generic;
using CutsceneEngine;
using UnityEngine;

namespace CutsceneEngineEditor
{
    internal static class HumanoidIKDigitPoseUtility
    {
        const float PoseMin = -1f;
        const float PoseMax = 1f;
        const int DigitCount = 5;

        public static int GetExistingJointCount(Transform[] chain)
        {
            if (chain == null) return 0;

            var count = 0;
            for (var i = 0; i < chain.Length; i++)
            {
                if (chain[i]) count++;
            }
            return Mathf.Min(count, 3);
        }

        public static float GetToeDigitPose(HumanoidIKJointBend bend, int jointCount)
        {
            return GetToeDigitPose(null, bend, jointCount);
        }

        public static float GetToeDigitPose(HumanoidIKClip clip, HumanoidIKJointBend bend, int jointCount)
        {
            if (jointCount <= 0) return 0f;

            var sum = 0f;
            for (var jointIndex = 0; jointIndex < jointCount; jointIndex++)
            {
                sum += GetToeJointPose(clip, bend, jointIndex);
            }
            return sum / jointCount;
        }

        public static float GetToeJointPose(HumanoidIKJointBend bend, int jointIndex)
        {
            return GetToeJointPose(null, bend, jointIndex);
        }

        public static float GetToeJointPose(HumanoidIKClip clip, HumanoidIKJointBend bend, int jointIndex)
        {
            var angle = jointIndex switch
            {
                0 => bend.proximal.x,
                1 => bend.intermediate.x,
                2 => bend.distal.x,
                _ => 0f
            };
            var range = GetToeBendRange(clip, jointIndex);
            return angle < 0f
                ? Mathf.Clamp(angle / Mathf.Max(Mathf.Abs(range.x), Mathf.Epsilon), PoseMin, 0f)
                : Mathf.Clamp(angle / Mathf.Max(range.y, Mathf.Epsilon), 0f, PoseMax);
        }

        public static void SetToeAllJointPose(ref HumanoidIKJointBend bend, int jointCount, float value)
        {
            SetToeAllJointPose(null, ref bend, jointCount, value);
        }

        public static void SetToeAllJointPose(HumanoidIKClip clip, ref HumanoidIKJointBend bend, int jointCount, float value)
        {
            for (var jointIndex = 0; jointIndex < jointCount; jointIndex++)
            {
                SetToeJointPose(clip, ref bend, jointIndex, value);
            }
        }

        public static void SetToeJointPose(ref HumanoidIKJointBend bend, int jointIndex, float value)
        {
            SetToeJointPose(null, ref bend, jointIndex, value);
        }

        public static void SetToeJointPose(HumanoidIKClip clip, ref HumanoidIKJointBend bend, int jointIndex, float value)
        {
            var range = GetToeBendRange(clip, jointIndex);
            value = Mathf.Clamp(value, PoseMin, PoseMax);
            var angle = value < 0f ? value * Mathf.Abs(range.x) : value * range.y;
            switch (jointIndex)
            {
                case 0:
                    bend.proximal.x = angle;
                    break;
                case 1:
                    bend.intermediate.x = angle;
                    break;
                case 2:
                    bend.distal.x = angle;
                    break;
            }
        }

        public static Vector2 GetToeBendRange(HumanoidIKClip clip, int jointIndex)
        {
            return OrderedRange(clip ? clip.GetToeBendRange(jointIndex) : HumanoidIKUtility.GetDefaultToeBendRange(jointIndex));
        }

        public static Vector2 GetToeBaseBendRange(HumanoidIKClip clip)
        {
            return OrderedRange(clip ? clip.GetToeBaseBendRange() : new Vector2(-25f, 20f));
        }

        public static float GetHandStretch(HumanoidIKClip clip, in HumanoidIKDigitBendPose pose)
        {
            var sum = 0f;
            for (var i = 0; i < DigitCount; i++)
            {
                sum += GetDigitPose(clip, GetDigitBend(in pose, i), i);
            }
            return sum / DigitCount;
        }

        public static float GetToeStretch(
            in HumanoidIKDigitBendPose pose,
            IReadOnlyList<Transform[]> chains,
            bool includeToeBase,
            float toeBasePose)
        {
            return GetToeStretch(null, in pose, chains, includeToeBase, toeBasePose);
        }

        public static float GetToeStretch(
            HumanoidIKClip clip,
            in HumanoidIKDigitBendPose pose,
            IReadOnlyList<Transform[]> chains,
            bool includeToeBase,
            float toeBasePose)
        {
            if (chains == null) return 0f;

            var sum = includeToeBase ? Mathf.Clamp(toeBasePose, PoseMin, PoseMax) : 0f;
            var controlCount = includeToeBase ? 1 : 0;
            var rowCount = Mathf.Min(DigitCount, chains.Count);
            for (var digitIndex = 0; digitIndex < rowCount; digitIndex++)
            {
                var jointCount = GetExistingJointCount(chains[digitIndex]);
                if (jointCount <= 0) continue;

                sum += GetToeDigitPose(clip, GetDigitBend(in pose, digitIndex), jointCount);
                controlCount++;
            }
            return controlCount > 0 ? sum / controlCount : 0f;
        }

        public static float GetFingerFanPose(HumanoidIKClip clip, in HumanoidIKDigitBendPose pose)
        {
            var sum = 0f;
            for (var digitIndex = 1; digitIndex < DigitCount; digitIndex++)
            {
                var bend = GetDigitBend(in pose, digitIndex);
                sum += GetFingerFanPoseFromAngle(clip, bend.proximal.y, digitIndex);
            }
            return sum / (DigitCount - 1f);
        }

        public static float GetDigitPose(HumanoidIKClip clip, HumanoidIKJointBend bend, int digitIndex)
        {
            return (
                GetJointPose(clip, bend, digitIndex, 0) +
                GetJointPose(clip, bend, digitIndex, 1) +
                GetJointPose(clip, bend, digitIndex, 2)) / 3f;
        }

        public static float GetJointPose(
            HumanoidIKClip clip,
            HumanoidIKJointBend bend,
            int digitIndex,
            int jointIndex)
        {
            var angle = jointIndex switch
            {
                0 => bend.proximal.x,
                1 => bend.intermediate.x,
                2 => bend.distal.x,
                _ => 0f
            };
            return GetPoseFromAngle(clip, angle, digitIndex, jointIndex);
        }

        public static void SetAllJointPose(
            HumanoidIKClip clip,
            ref HumanoidIKJointBend bend,
            int digitIndex,
            float value)
        {
            bend.proximal.x = GetAngleFromPose(clip, value, digitIndex, 0);
            bend.intermediate.x = GetAngleFromPose(clip, value, digitIndex, 1);
            bend.distal.x = GetAngleFromPose(clip, value, digitIndex, 2);
        }

        public static void SetJointPose(
            HumanoidIKClip clip,
            ref HumanoidIKJointBend bend,
            int digitIndex,
            int jointIndex,
            float value)
        {
            switch (jointIndex)
            {
                case 0:
                    bend.proximal.x = GetAngleFromPose(clip, value, digitIndex, jointIndex);
                    break;
                case 1:
                    bend.intermediate.x = GetAngleFromPose(clip, value, digitIndex, jointIndex);
                    break;
                case 2:
                    bend.distal.x = GetAngleFromPose(clip, value, digitIndex, jointIndex);
                    break;
            }
        }

        public static float GetAngleFromPose(HumanoidIKClip clip, float pose, int digitIndex, int jointIndex)
        {
            var range = GetJointRange(clip, digitIndex, jointIndex);
            var t = Mathf.InverseLerp(PoseMin, PoseMax, Mathf.Clamp(pose, PoseMin, PoseMax));
            return Mathf.Lerp(range.x, range.y, t);
        }

        public static float GetPoseFromAngle(HumanoidIKClip clip, float angle, int digitIndex, int jointIndex)
        {
            var range = GetJointRange(clip, digitIndex, jointIndex);
            var value = Mathf.Clamp(angle, range.x, range.y);
            var angleRange = range.y - range.x;
            var t = Mathf.Approximately(angleRange, 0f)
                ? 0f
                : Mathf.Clamp01((value - range.x) / angleRange);
            return Mathf.Lerp(PoseMin, PoseMax, t);
        }

        public static Vector2 GetJointRange(HumanoidIKClip clip, int digitIndex, int jointIndex)
        {
            return OrderedRange(clip.GetDigitBendRange(digitIndex, jointIndex));
        }

        public static Vector2 GetThumbSpreadRange(HumanoidIKClip clip)
        {
            return OrderedRange(clip.GetThumbSpreadRange());
        }

        public static float GetFingerSpreadAngleFromPose(HumanoidIKClip clip, float pose, int digitIndex)
        {
            var range = GetFingerSpreadRange(clip, digitIndex);
            var t = Mathf.InverseLerp(PoseMin, PoseMax, Mathf.Clamp(pose, PoseMin, PoseMax));
            return Mathf.Lerp(range.x, range.y, t);
        }

        public static float GetFingerFanPoseFromAngle(HumanoidIKClip clip, float angle, int digitIndex)
        {
            var range = GetFingerSpreadRange(clip, digitIndex);
            var angleRange = range.y - range.x;
            var t = Mathf.Approximately(angleRange, 0f)
                ? 0.5f
                : Mathf.Clamp01((angle - range.x) / angleRange);
            return Mathf.Lerp(PoseMin, PoseMax, t);
        }

        public static Vector2 GetFingerSpreadRange(HumanoidIKClip clip, int digitIndex)
        {
            return OrderedRange(clip.GetFingerSpreadRange(digitIndex));
        }

        public static HumanoidIKJointBend GetDigitBend(in HumanoidIKDigitBendPose pose, int digitIndex)
        {
            return HumanoidIKUtility.GetDigitBend(in pose, digitIndex);
        }

        public static void SetDigitBend(
            ref HumanoidIKDigitBendPose pose,
            int digitIndex,
            HumanoidIKJointBend bend)
        {
            switch (digitIndex)
            {
                case 0:
                    pose.thumbOrBigToe = bend;
                    break;
                case 1:
                    pose.indexOrSecondToe = bend;
                    break;
                case 2:
                    pose.middleOrThirdToe = bend;
                    break;
                case 3:
                    pose.ringOrFourthToe = bend;
                    break;
                case 4:
                    pose.littleOrFifthToe = bend;
                    break;
            }
        }

        static Vector2 OrderedRange(Vector2 range)
        {
            return new Vector2(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
        }
    }
}
