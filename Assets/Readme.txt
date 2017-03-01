HEIZO開発で作ったプログラム
システムで使っていない物もありますが参考として一応追加しています．
元になったRocatのプログラム等が一切ドキュメントない状態だった上に，
一ノ瀬のプログラミング力の無さが加わって
かなり構造がテキトーになっていますが許してください…

都市の生成の中心となるプログラムはCameraMove.csとCityCreater.csです．

ココではざっくりとしか説明しませんので，各ファイルのコメントも合わせてご覧ください．


レーダーの開発の参考はココ
https://www.youtube.com/watch?v=O8is_EikILA

リスト作成の参考はココ
http://geekhack.mods.jp/geek-hack/2016/01/03/post-59/


使用したアセット
https://www.assetstore.unity3d.com/jp/#!/content/67413
https://www.assetstore.unity3d.com/jp/#!/content/13811
https://www.assetstore.unity3d.com/jp/#!/content/11117



・Block.cs
土台（ディレクトリを示す）のクラス
ココでは主にオンマウス時の色変更をしてます．


・BlockData.cs
土台に付与するメタデータのクラス
開発後半に急遽付け足したモノですがたぶんBlock.csと統合できますねコレ…


・Building.cs
ビル（ファイルを示す）のクラス
Block.cs同様，主にオンマウス時の色変更をしてます．


・BuilingData.cs
ビルに付与するメタデータのクラス
コレもBuilding.csと統合できそう


・CameraMove.cs
カメラ（視点）の移動やマウス・キーボード操作関連のクラス
カメラ操作ができるようになった時点で動かすメソッドなどもココから呼び出している物が多いです


・CityCreater.cs
都市を作るためのクラス
オブジェクトの配置決定やインスタンス生成をしています．


・DirName.cs
土台上に表示されるディレクトリ名のサイズ調整や角度調整をするクラス


・FileName.cs
SATDのあるファイルのビルに表示される名前のサイズ調整や角度調整をするクラス


・FireBehaviour.cs
炎のエフェクトをカメラとの距離に応じて出したり消したりするクラス
最終的に使いませんでしたが「距離に応じてオブジェクトを表示/非表示にする」という機構は使いませるかと


・ItemListManager.cs
SATDのあるファイルのリストの項目を生成するクラス
SATDのあるファイルのビルの情報を一つ一つノードに設定していっています
Jsonファイルの読み込み後でないと実行できないのでOnGUIで無理やり1回だけ動かしてますが別の良い方法があればそちらにした方が良いはず
上に書いたリスト作成の参考ページも見てみてください


・MiniJSON.cs
Jsonファイルを簡単に扱うためのライブラリ


・MoveBehaviour.cs
ビルをカメラとの距離に応じて出したり引っ込めたりするためのクラス
最終的に使いませんでしたが「距離に応じてオブジェクトを動かす」という機構の参考にどうぞ．


・MoveDistance.cs
↑の処理を呼び出すためのクラス


・MapCameraMove.cs
マップ用のカメラ関連のクラス


・RaderCursorMove.cs
レーダーに映る自分の位置（カーソル）の表示をするためのクラス


・Sensor.cs
レーダーに映る点の生成や描画をするクラス


・SpriteChange.cs
レーダーの背景色を決めるクラス
正直不要…


・UpperButton.cs
1個上の階層に戻るボタンのクラス
クリックしたときにCityCreaterのメソッドを呼び出して街を再構成しています．