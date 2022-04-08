using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eScoreEventPy{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

public class ScoreManagerPy : MonoBehaviour
{
    static private ScoreManagerPy S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    public int          chain = 0;
    public int          scoreRun = 0;
    public int          score = 0;

    void Awake(){
        if(S == null){
            S = this;
        }else{
            Debug.LogError("ERROR: ScoreManagerPy.Awake(): S is already set!");
        }

        if(PlayerPrefs.HasKey("ProspectorHighScore")){
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(eScoreEventPy evt){
        try{
            S.Event(evt);
        }catch(System.NullReferenceException nre){
            Debug.LogError("ScoreManagerPy:EVENT() called while S = null.\n"+nre);
        }
    }

    void Event(eScoreEventPy evt){
        switch(evt){
            case eScoreEventPy.draw:
            case eScoreEventPy.gameWin:
            case eScoreEventPy.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;

            case eScoreEventPy.mine:
                chain++;
                scoreRun += chain;
                break;
        }

        switch(evt){
            case eScoreEventPy.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round Score: "+score);
                
                break;

            case eScoreEventPy.gameLoss:
                
                if(HIGH_SCORE <= score){
                    print("You got the high score! High Score: "+score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }else{
                    print("Your final score for the game was: "+score);
                }
                
                break;
            
            default:
                print("score: "+score+" scoreRun: "+scoreRun+" chain: "+chain);
                break;
        }
    }

    static public int CHAIN{get{return S.chain;}}
    static public int SCORE{get{return S.score;}}
    static public int SCORE_RUN{get{return S.scoreRun;}}
}
