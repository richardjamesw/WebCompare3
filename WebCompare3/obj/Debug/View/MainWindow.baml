   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5   	   F BWebCompare3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:WebCompare3WebCompare3 5	   	      WebCompare3.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   6sys&clr-namespace:System;assembly=mscorlib     5   	   +localclr-namespace:WebCompare3     5	   	     � Title$  
WebCompare��5
   	   $	��500��6   $	��800��6)      WindowBackgroundBrush#���   5   	   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   .+�5   
   ��   BasicTextStyle#���  6   $!��Enter a URL for comparison:��6=   $��20, 0, 0, 0q�6`   � 5   
   $��1��6   .+�5      �� -   5        $��txtURL��5      $	��250��6=   $��Center=�6I   $��Center=�6f   $��20, 5, 5, 5q�6�   ��6$   �� *	UserURL+6�   �� 5      
�����p�$��3q�6U    �� CornerRadius$ 50��6i   $��40��6{   $��40��6�   $��0q�6�   {�6   ��  �� ElementName$ btnGo����5      �� 5       �� 
TargetType" ��6   .�5       ��  �� Property � �� �� Value
 ��� �67    �� Triggers 5!      �� 5"      	 �� Binding	 6*   �� $ btnGo��
 �� Path$
 IsMouseOver� �� Value True6b   .a�5#   "   ��  � ��  TransparentBack# �  6?   5$      �� 5%      	 6*   �� $ btnGo��$
 IsMouseDown� True6b   .a�5&   "   ��  � ��# �  6?   5'      5)      ._�5+      �� -   5        $��btnGo��5+        ButtonTransparent#���  6D   ��6&   �� *	GoCommand+.��5,      �� $-��'/WebCompare2.0;component/View/start.png��6   $
��None=�6M   $��-2,0,1,0q�6\   5-      5.      5/      �� 53   
   -   5         ��Name$ lstWebsites��53      $y�	Collapsed=�6'   $��1��6c   $��2��6s   $��Right=�6�   $��Stretch=�6�   $	��300��6�   $��10q�6�   ��6>   �� *ListWebsites+6�   �� 56   
   -   5         ��Name$ 
txtDataRcv��56       �� 
IsReadOnly . $��0��6^   $��2��6n   $��Center=�6{   $��Stretch=�6�   $��10q�6�   $	��350��6�    �� TextWrapping$
 Wrap=�6�   ��6&   �� *	Results+ �� Mode$ OneWay=�6�   �� 59   
   -   5        $��tbStatus��59       . $��1��6{   $��2��6�   " �� VerticalScrollBarVisibility$
 Auto=�6�   $��Center=�6�   $��Stretch=�6�   $��10q�6�   $	��350��6  $
 Wrap=�6  ��6D   �� *Status+$ OneWay=�6  5>      5@      