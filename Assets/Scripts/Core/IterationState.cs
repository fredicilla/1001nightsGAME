using System.Collections.Generic;
using UnityEngine;
using GeniesGambit.Player;
using GeniesGambit.Combat;

namespace GeniesGambit.Core
{
    [System.Serializable]
    public class IterationState
    {
        public int iteration;
        public Vector3 heroPosition;
        public Vector3 enemyPosition;
        public List<FrameData> heroRecording;
        public List<ShootEvent> heroShooterRecording;
        public List<FrameData> enemyRecording;
        public List<ShootEvent> enemyShooterRecording;
        public bool heroReachedFlag;
        public bool enemyKilledGhost;

        public IterationState(
            int iteration,
            Vector3 heroPosition,
            Vector3 enemyPosition,
            List<FrameData> heroRecording,
            List<ShootEvent> heroShooterRecording,
            List<FrameData> enemyRecording,
            List<ShootEvent> enemyShooterRecording,
            bool heroReachedFlag,
            bool enemyKilledGhost)
        {
            this.iteration = iteration;
            this.heroPosition = heroPosition;
            this.enemyPosition = enemyPosition;
            this.heroRecording = heroRecording;
            this.heroShooterRecording = heroShooterRecording;
            this.enemyRecording = enemyRecording;
            this.enemyShooterRecording = enemyShooterRecording;
            this.heroReachedFlag = heroReachedFlag;
            this.enemyKilledGhost = enemyKilledGhost;
        }

        public IterationState Clone()
        {
            return new IterationState(
                iteration,
                heroPosition,
                enemyPosition,
                heroRecording != null ? new List<FrameData>(heroRecording) : null,
                heroShooterRecording != null ? new List<ShootEvent>(heroShooterRecording) : null,
                enemyRecording != null ? new List<FrameData>(enemyRecording) : null,
                enemyShooterRecording != null ? new List<ShootEvent>(enemyShooterRecording) : null,
                heroReachedFlag,
                enemyKilledGhost
            );
        }
    }
}
