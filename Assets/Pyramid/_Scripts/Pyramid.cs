using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Pyramid : MonoBehaviour {

	static public Pyramid 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset			layoutXML;
	public float 				xOffset = 3;
	public float 				yOffset = -2.5f;
	public Vector3				layoutCenter;
	public Vector2				fsPosMid = new Vector2(0.5f, 0.90f);
	public Vector2				fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2				fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2				fsPosEnd = new Vector2(0.5f, 0.95f);
	public float				reloadDelay = 5f;
	public Text					gameOverText, roundResultText, highScoreText;


	[Header("Set Dynamically")]
	public Deck						deck;
	public LayoutPy					layout;
	public List<CardPyramid>		drawPile;
	public Transform				layoutAnchor;
	public CardPyramid				target;
	public List<CardPyramid>		tableau;
	public List<CardPyramid>		discardPile;
	public FloatingScore			fsRun;

	void Awake(){
		S = this;
		SetUpUITexts();
	}

	void SetUpUITexts(){
		GameObject go = GameObject.Find("HighScore");

		if(go != null){
			highScoreText = go.GetComponent<Text>();
		}

		int highScore = ScoreManagerPy.HIGH_SCORE;
		string hScore = "High Score: "+Utils.AddCommasToNumber(highScore);
		go.GetComponent<Text>().text = hScore;

		go = GameObject.Find("GameOver");
		if(go != null){
			gameOverText = go.GetComponent<Text>();
		}

		go = GameObject.Find("RoundResult");
		if(go != null){
			roundResultText = go.GetComponent<Text>();
		}

		ShowResultsUI(false);
	}

	void ShowResultsUI(bool show){
		gameOverText.gameObject.SetActive(show);
		roundResultText.gameObject.SetActive(show);
	}

	void Start() {
		ScoreBoardPy.S.score = ScoreManagerPy.SCORE;

		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);

		
		Deck.Shuffle(ref deck.cards);
		
		//Card c;
		//for (int cNum = 0; cNum<deck.cards.Count; cNum++){
		//	c = deck.cards[cNum];
		//	c.transform.localPosition = new Vector3((cNum%13)*3, cNum/13*4, 0);
		//}
		
		layout = GetComponent<LayoutPy> ();
		layout.ReadLayout(layoutXML.text);

		drawPile = ConvertListCardsToListCardPyramids(deck.cards);

		LayoutGame();
	}

	List<CardPyramid> ConvertListCardsToListCardPyramids(List<Card> lCD){
		List<CardPyramid> lCP = new List<CardPyramid>();
		CardPyramid tCP;
		
		foreach(CardPyramid tCD in lCD){
			tCP = tCD as CardPyramid;
			lCP.Add(tCP);
		}

		return(lCP);
	}

	CardPyramid Draw(){
		
		CardPyramid cd = drawPile[0];
		drawPile.RemoveAt(0);
		
		return(cd);
	}

	void LayoutGame(){
		
		if(layoutAnchor == null){
			GameObject tGO = new GameObject("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}

		CardPyramid cp;
		foreach(SlotDefPy tSD in layout.slotDefs){
			cp = Draw();
			cp.faceUp = tSD.faceUp;
			cp.transform.parent = layoutAnchor;
			
			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layerID);

			cp.layoutID = tSD.id;
			cp.slotDefPy = tSD;
			cp.state = ePyCardState.tableau;

			cp.SetSortingLayerName(tSD.layerName);

			tableau.Add(cp);
		}

		foreach(CardPyramid tCP in tableau){
			foreach(int hid in tCP.slotDefPy.hiddenBy){
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}
		}

		MoveToTarget(Draw());
		UpdateDrawPile();
	}

	CardPyramid FindCardByLayoutID(int layoutID){
		foreach(CardPyramid tCP in tableau){
			if(tCP.layoutID == layoutID){
				return(tCP);
			}
		}

		return(null);
	}

	void SetTableauFaces(){
		foreach(CardPyramid cd in tableau){
			bool faceUp = true;

			foreach(CardPyramid cover in cd.hiddenBy){
				if(cover.state == ePyCardState.tableau){
					faceUp = false;
				}
			}

			cd.faceUp = faceUp;
		}
	}

	void MoveToDiscard(CardPyramid cd){
		cd.state = ePyCardState.discard;
		discardPile.Add(cd);
		cd.transform.parent = layoutAnchor;

		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID + 0.5f
		);

		cd.faceUp = true;
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100+discardPile.Count);
	}

	void MoveToTarget(CardPyramid cd){
		if(target != null) MoveToDiscard(target);
		target = cd;

		cd.state = ePyCardState.target;
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID
		);

		cd.faceUp = true;
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(0);
	}

	void UpdateDrawPile(){
		CardPyramid cd;
		
		for (int i=0; i<drawPile.Count; i++){
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;

			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.drawPile.x + i*dpStagger.x),
				layout.multiplier.y * (layout.drawPile.y + i*dpStagger.y),
				-layout.drawPile.layerID+0.1f*i
			);

			cd.faceUp = false;
			cd.state = ePyCardState.drawpile;
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10*i);
		}
	}

	public void CardClicked(CardPyramid cd){
		
		switch (cd.state){
			case ePyCardState.target:
				break;

			case ePyCardState.drawpile:
				MoveToDiscard(target);
				MoveToTarget(Draw());
				UpdateDrawPile();
				ScoreManagerPy.EVENT(eScoreEventPy.draw);
				FloatingScoreHandler(eScoreEventPy.draw);
				break;

			case ePyCardState.tableau:
				bool validMatch = true;
				
				if(!cd.faceUp){
					validMatch = false;
				}
				
				if(!CheckRank(cd,target)){
					validMatch = false;
				}

				if(!validMatch) return;

				tableau.Remove(cd);
				MoveToTarget(cd);	
				SetTableauFaces();	
				ScoreManagerPy.EVENT(eScoreEventPy.mine);
				FloatingScoreHandler(eScoreEventPy.mine);		
				break;
		}
		
		CheckForGameOver();
	}

	void CheckForGameOver(){
		if(tableau.Count == 0){
			GameOver(true);
			return;
		}

		if (drawPile.Count>0){
			return;
		}

		foreach(CardPyramid cd in tableau){
			if(CheckRank(cd, target)){
				return;
			}
		}

		GameOver(false);
	}

	void GameOver(bool won){
		int score = ScoreManagerPy.SCORE;

		if(fsRun != null) score += fsRun.score;


		if(won){
			//print("Game Over. You won! :)");

			gameOverText.text = "Round Over";
			roundResultText.text = "You won this round!\nRoundScore: "+score;
			ShowResultsUI(true);

			ScoreManagerPy.EVENT(eScoreEventPy.gameWin);
			FloatingScoreHandler(eScoreEventPy.gameWin);
		}else{
			//print("Game Over, You lost. :(");

			gameOverText.text = "Game Over";
			if(ScoreManagerPy.HIGH_SCORE <= score){
				string str = "You got the high score!\nHigh Score: "+score;
				roundResultText.text = str;
			}else{
				roundResultText.text = "Your final score was: "+score;
			}
			ShowResultsUI(true);

			ScoreManagerPy.EVENT(eScoreEventPy.gameLoss);
			FloatingScoreHandler(eScoreEventPy.gameLoss);
		}

		//SceneManager.LoadScene("__Prospector_Scene_0");
		Invoke("ReloadLevel", reloadDelay);
	}

	void ReloadLevel(){
		SceneManager.LoadScene("Pyramid Solitaire");
	}

	public bool CheckRank(CardPyramid c0, CardPyramid c1){
		if(!c0.faceUp || !c1.faceUp) return(false);

		/*if(Mathf.Abs(c0.rank - c1.rank) == 1){
			return(true);
		}*/

		if(c0.rank + c1.rank == 13) return(true);
		if(c0.rank == 13 || c1.rank == 13) return(true);

		return(false);
	}

	void FloatingScoreHandler(eScoreEventPy evt){
		List<Vector2> fsPts;
		switch(evt){
			
			case eScoreEventPy.draw:
			case eScoreEventPy.gameWin:
			case eScoreEventPy.gameLoss:
				
				if(fsRun != null){
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);
					fsRun.reportFinishTo = ScoreBoardPy.S.gameObject;
					fsRun.Init(fsPts, 0, 1);
					fsRun.fontSizes = new List<float>(new float[] {28,36,4});
					fsRun = null;
				}

			break;

			case eScoreEventPy.mine:
				FloatingScore fs;
				
				Vector2 p0 = Input.mousePosition;

				p0.x /= Screen.width;
				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);
				fs = ScoreBoardPy.S.CreateFloatingScore(ScoreManagerPy.CHAIN, fsPts);
				fs.fontSizes = new List<float>(new float[] {4, 50, 28});

				if(fsRun == null){
					fsRun = fs;
					fsRun.reportFinishTo = null;
				}else{
					fs.reportFinishTo = fsRun.gameObject;
				}
				
				break;
		}
	}
}
