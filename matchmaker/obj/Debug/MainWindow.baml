   M S B A M L   `   `   `  ����E  Amatchmaker, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:matchmaker
matchmaker  5   	       matchmaker.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:matchmaker     5   	     � Title$  
MainWindow��5   	   $	��350��6   $	��525��6)   .��5	      � -   5         �Name$ 
gRessource��5	       � 	Resources 5
   
   ��%    5      .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ dgridPlayers��5         AutoGenerateColumns .  $��
0,32,0,-32q�6E      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$ Id��6%   1  +System.Windows.Controls.DataGridBoundColumn   Binding 61   �� *Id+  5      $ Estimate skill��6%    6=   �� *EstimateSkill+  5      $ 
Real Skill��6%    69   �� *	RealSkill+5      �� 5   
   -   5         ��Name$ btnGeneratePlayers��5      $��Generate��6-   $
��Left=�6@   $��18,7,0,0q�6[   $	��Top=�6m   $��75��6�   6�   �� 5   
   -   5        $ btnSimulate��5      $��Simulate��6&   $
��Left=�69   $��	113,7,0,0q�6T   $	��Top=�6g   $��75��6   6�   5      5      