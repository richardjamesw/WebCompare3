   M S B A M L   `   `   `  ����F  BWebCompare3, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:WebCompare3.ViewWebCompare3.View  5   	       WebCompare3.View.Loader   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:WebCompare3.View     5   	     � Title$  Loader��5	   	   $	��450��6   $	��300��6%   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��* �6   .+�5   
   �� $��5,0,0,0q�6   $
��Wrap=�6%   	Select �� 6A   .�6F   Load6L   +) to load all stored program data. Select �� 5      .�6   Start6!    to begin program.6:   �� 5   
   $��1��69   $��Center=�6F   $��Center=�6c    d� 	IsChecked 6   �� *UpdateIsChecked+.��6~   Update6�   �� 5   
   $
��Left=�61   $��50��6L   $��20��6W   $��30,0,0,0q�6c   
����� �$��1��6�   ��6   �� *LoadCommand+.��6�   Load6�   �� 5   
   $��Right=�62   $��50��6N   $��20��6Y   $��0,0,30,0q�6e   
����� �$��1��6�   ��6   �� *StartCommand+.��6�   Start6�   �� 5   
   -   5        $��tbLoaderStatus��5       �� 
IsReadOnly .  $��3��6�   " �� VerticalScrollBarVisibility$
 Auto=�6�    �� TextWrapping$
 Wrap=�6�   ��6V   �� *
LoadStatus+6�   5!      5"      