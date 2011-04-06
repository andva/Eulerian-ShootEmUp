using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModel;

namespace SkinnedModel
{
    public class ClipPlayer
    {
        Matrix[] boneTransforms, skinTransforms, worldTransforms;
        AnimationClip currentClip;
        IList<Keyframe> keyframeList;
        SkinningData skinData;
        float fps;
        TimeSpan startTime, endTime, currentTime;
        TimeSpan startTimeSwitch, endTimeSwitch;
        bool isLooping;
        bool isSwitching;
        public bool playing = true;
        float blend = 0;

        public ClipPlayer(SkinningData skd, float fps)
        {
            skinData = skd;
            boneTransforms = new Matrix[skd.BindPose.Count];
            skinTransforms = new Matrix[skd.BindPose.Count];
            worldTransforms = new Matrix[skd.BindPose.Count];
            this.fps = fps;
        }

        public void ChangeModel(SkinningData skd)
        {
            skinData = skd;
            boneTransforms = new Matrix[skd.BindPose.Count];
            skinTransforms = new Matrix[skd.BindPose.Count];
            worldTransforms = new Matrix[skd.BindPose.Count];
        }

        public void play(AnimationClip clip, float startFrame, float endFrame, bool loop)
        {
            this.currentClip = clip;
            startTime = TimeSpan.FromMilliseconds(startFrame / fps * 1000);
            endTime = TimeSpan.FromMilliseconds(endFrame / fps * 1000);
            currentTime = startTime;
            isLooping = loop;
            keyframeList = currentClip.Keyframes;
        }
        public void switchRange(float s, float e)
        {
            isSwitching = true;
            startTimeSwitch = TimeSpan.FromMilliseconds(s / fps * 1000);
            endTimeSwitch = TimeSpan.FromMilliseconds(e / fps * 1000);
        }

        public bool inRange(float s, float e)
        {
            TimeSpan sRange = TimeSpan.FromMilliseconds(s / fps * 1000);
            TimeSpan eRange = TimeSpan.FromMilliseconds(e / fps * 1000);
            if (currentTime >= sRange && currentTime <= eRange)
                return true;
            else
                return false;
        }
        public Matrix getWorldTransform(int id)
        {
            return worldTransforms[id];
        }

        public Matrix[] GetTransformsFromTime(TimeSpan ts)
        {
            Matrix[] xfroms = new Matrix[skinData.BindPose.Count];
            skinData.BindPose.CopyTo(xfroms, 0);
            int keyNum = 0;
            while (keyNum < keyframeList.Count)
            {
                Keyframe key = keyframeList[keyNum];
                if (key.Time > ts) break;
                xfroms[key.Bone] = key.Transform;
                keyNum++;
            }
            return xfroms;
        }
        public Matrix[] GetTransformsFromTime(float a)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(a / fps * 1000);
            return GetTransformsFromTime(ts);
        }
        public Matrix[] BlendTransforms(Matrix[] fromTransforms,
                                        Matrix[] toTransforms)
        {
            for (int i = 0; i < fromTransforms.Length; i++)
            {
                Vector3 vt1; Vector3 vs1; Quaternion q1;
                fromTransforms[i].Decompose(out vs1, out q1, out vt1);

                Vector3 vt2; Vector3 vs2; Quaternion q2;
                toTransforms[i].Decompose(out vs2, out q2, out vt2);

                Vector3 vtBlend = Vector3.Lerp(vt1, vt2, blend);
                Vector3 vsBlend = Vector3.Lerp(vs1, vs2, blend);
                Quaternion qBlend = Quaternion.Slerp(q1, q2, blend);

                toTransforms[i] = Matrix.CreateScale(vsBlend) *
                            Matrix.CreateFromQuaternion(qBlend) *
                            Matrix.CreateTranslation(vtBlend);
            }
            return toTransforms;
        }
        public void update(TimeSpan time, bool relative, Matrix root)
        {
            if (playing)
            {
                if (relative)
                    currentTime += time;
                else
                    currentTime = time;

                boneTransforms = GetTransformsFromTime(currentTime);

                if (currentTime >= endTime)
                {
                    if (isLooping)
                    {
                        currentTime = startTime;
                    }
                    else
                    {
                        currentTime = endTime;
                    }
                }

                if (isSwitching)
                {
                    blend += 0.1f;
                    boneTransforms = BlendTransforms(boneTransforms,
                                    GetTransformsFromTime(startTimeSwitch));
                    if (blend > 1)
                    {
                        isSwitching = false;
                        startTime = startTimeSwitch;
                        endTime = endTimeSwitch;
                        currentTime = startTime;
                        blend = 0;
                    }

                }

                worldTransforms[0] = boneTransforms[0] * root;

                for (int i = 1; i < worldTransforms.Length; i++)
                {
                    int parent = skinData.SkeletonHierarchy[i];
                    worldTransforms[i] = boneTransforms[i] * worldTransforms[parent];

                }
                for (int i = 0; i < skinTransforms.Length; i++)
                {
                    skinTransforms[i] = skinData.InverseBindPose[i] * worldTransforms[i];
                }
            }
        }
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }
        public void updateFps(float fps)
        {
            this.fps = fps;
        }
    }
}
