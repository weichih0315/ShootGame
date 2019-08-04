using UnityEngine;
using System.Collections;

public class ScoreKeeper : MonoBehaviour {

	public static int score { get; private set; }
	float lastEnemyKillTime;
	int streakCount;
	float streakExpiryTime = 1;

	void Start() {
		Enemy.OnDeathStatic += OnEnemyKilled;
		FindObjectOfType<Player> ().OnDeath += OnPlayerDeath;
	}

	void OnEnemyKilled() {

        /*  連續擊殺獎勵
		if (Time.time < lastEnemyKillTime + streakExpiryTime) {
			streakCount++;
		} else {
			streakCount = 0;
		}

		lastEnemyKillTime = Time.time;

		score += 5 + (int)Mathf.Pow(2,streakCount);
        */

        score += 10;
    }

	void OnPlayerDeath() {
		Enemy.OnDeathStatic -= OnEnemyKilled;
	}
}
