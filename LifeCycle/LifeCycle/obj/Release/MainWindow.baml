   M S B A M L   `   `   `  ����   	LifeCycle    LifeCycle.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     \ XMicrosoft.Kinect.Toolkit.Controls, Version=0.0.0.0, Culture=neutral, PublicKeyToken=nullS OMicrosoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null2k(http://schemas.microsoft.com/kinect/2013   <d2http://schemas.microsoft.com/expression/blend/2008   Fmc;http://schemas.openxmlformats.org/markup-compatibility/2006     � Title$  
MainWindow��$	��768��$
��1024�����	#FFF7F1F1 � 
ResizeMode$ CanMinimize=�.��� $��0,10,2,0q�.+�4  .Microsoft.Kinect.Toolkit.KinectSensorChooserUI  -     Name$ sensorChooserUi��$��Center=�$	��Top=�4  .Microsoft.Kinect.Toolkit.Controls.KinectRegion  -     Name$ kinectRegion��$��	0,-9,0,39q����	#FF746E6E.��� $��53,10,10,-10q�.+�� $
��Left=�$	��411��$��-52,-10,0,-28q����	#FF01A2E8 � RowDefinitions �� $
��950* ��� $
��223* �.+��� $��4q�
�������$	��Top=�$
��Left=�$��
10,10,0,10q�$	��240��$	��320��._��� -    ��Name$ kinectPatientFeed��$��Stretch=�$��Stretch=�$
��Fill=�$	��320��$	��240��$��-4q� J� OpacityMask
 �   �   KinectRegion ��  �� ElementName$ kinectRegion���� $��4q�
�������$��Bottom=�$
��Left=�$��
10,10,0,10q�$	��240��$	��320��$��2��._��� -   $ kinectClinitianFeed��$��Stretch=�$��Stretch=�$
��Fill=�$	��320��$	��240��$��-4q� �� $ kinectRegion��8  2Microsoft.Kinect.Toolkit.Controls.KinectTileButton  -   	  Name$	 showOptionsButton��$��Options��
   LabelBackground
 �#91908A$
��Left=�$��
60,294,0,0q�$	��Top=�$	��250��$	��150��
����������	#FF01A2E8$��48(�$��Microsoft Uighur)�� $
��Left=�$	��737��$	��Top=�$	��603��$��359,-10,-9,-28q�
������� �� $
��278* ��� $
��176* ��� $
��281* ��� $
��438* �.+�  -   $	 beginWorkoutButton��$��Begin Workout��
 �#91908A$
��Left=�$��10,113.419,0,0q�$	��Top=�$	��250��$	��150�����	#FF01A2E8
�������$��3��$��48(�$��Microsoft Uighur)�  -   $	 exitProgramButton��$
��Exit��$
��Left=�

 �**��$��343,113.419,0,0q�$	��250��$	��150�����	#FF01A2E8
�������$��3��$��48(�$��Microsoft Uighur)�$	��Top=��� $��	LifeCycle��$
��Left=�$��10,0,0,0q�$	��Top=�$	��100(�$
��Bold"����	#FF01A2E8$��Microsoft Uighur)�$	��118��$	��295���� -    ��Name$  startWorkoutCountdownLabel��$�� ��$��292,7,240.597,179q�$��Center=�$��Center=�$��64(�$��Black"����	#FFDE6D0F$	��419��$��2��$��95��  -	   $	 closeOptionsButton��$
��Done��
 �#91908A$
��Left=�$��343,109.419,0,0q�$	��Top=�$	��250��$	��150�����	#FF01A2E8
�������$��3��$y�Hidden=��� -
   $ heartRateLabel��$�� ��$
��Left=�$��204,51.419,0,0q�$��3��$	��Top=�$
��Bold"�$��48(����	#FF01A2E8$��Microsoft Uighur)�$��62��$	��134���� -   $ oxygenSatLabel��$�� ��$
��Left=�$��409,51.419,0,0q�$��3��$	��Top=�$
��Bold"�$��48(����	#FF01A2E8$��Microsoft Uighur)�$��62��$	��161���� -   $ 
timerLabel��$��
timerLabel��$
��Left=�$��29,119.419,0,0q�$	��Top=�$��64(�$
��Bold"�
����������	#FF01A2E8$��2��$��88��$	��331��$��2��:  4Microsoft.Kinect.Toolkit.Controls.KinectScrollViewer  -     Name$ selectTimeScrollViewer��$��Disabled=�$��Hidden=�$
��Left=�$��0,30,0,0q�$	��Top=�$	��195��$	��570�����	#FF01A2E8$��2��$��1��$y�Hidden=�   HoverBackground ��  �� EndPoint$ 0.5,1&� �� 
StartPoint$ 0.5,0&�.���  � Color$ Black�� � Offset$ 0Q�� $ White��$ 1Q�.���� -    ��Name$ workoutTimeScrollContent�� �� Orientation$ 
Horizontal=��� -   $ optionsTimeLabel��$'��!Select workout length in minutes:��$
��Left=�$��
94,203,0,0q�$	��Top=�$��55��$	��484��$��32(����	#FF01A2E8$y�Hidden=��� -   $ heartRateLabel_Copy��$��heartrate (BPM):��$
��Left=�$��10,56.419,0,0q�$��3��$	��Top=�$
��Bold"�$��36(����	#FF01A2E8$��Microsoft Uighur)�$��62��$	��194���� -   $ heartRateLabel_Copy1��$��Sats:��$
��Left=�$��343,56.419,0,0q�$��3��$	��Top=�$
��Bold"�$��36(����	#FF01A2E8$��Microsoft Uighur)�$��62��$��60��