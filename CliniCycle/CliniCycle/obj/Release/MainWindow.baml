   M S B A M L   `   `   `  ����   
CliniCycle    CliniCycle.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     _ [Microsoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35h dMicrosoft.Kinect.Toolkit.Controls, Version=1.8.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e352k(http://schemas.microsoft.com/kinect/2013   <d2http://schemas.microsoft.com/expression/blend/2008   Fmc;http://schemas.openxmlformats.org/markup-compatibility/2006   -     �Name$  Window�� � Title$ 
MainWindow��$
��1024��$	��768�� � 
ResizeMode$ CanMinimize=�.��� $��0,10,2,0q�.+�4  .Microsoft.Kinect.Toolkit.KinectSensorChooserUI  -   $��sensorChooserUi��$��Center=�$	��Top=�4  .Microsoft.Kinect.Toolkit.Controls.KinectRegion  -   $��kinectRegion��.��� -    �Name$ 
LayoutRoot�� J� OpacityMask
 �   � J� RenderTransformOrigin$ 
0.478,0.52&�
�������$��0,0,2,0q� � ColumnDefinitions �� $
��257* ��� $
��757* ��� $��0* � � RowDefinitions �� $	��19* ��� $
��141* �.+��� -    ��Name$ borderPatient1��$��4q����	#FF01A2E8$	��Top=�$
��Left=�$��	10,15,0,0q�$	��120��$	��160��$��1��._��� -   	 ��Name$	 kinectPatientFeed��$��Stretch=�$��Stretch=�$
��Fill=�$��-4q�$	��160��$	��120��
   KinectRegion
 ��  �� ElementName$ kinectRegion���� -   $ borderPatientLarge��$��4q����	#FF01A2E8$	��Top=�$
��Left=�$��107,141,0,0q�$	��480��$	��640��$��1��$��1��._��� -   $	 kinectPatientFeedLarge��$��Stretch=�$��Stretch=�$
��Fill=�$��-5,-2,-3,-6q�$	��640��$	��480��
 �� $ kinectRegion���� -	   $ borderPatient2��$��4q����	#FF01A2E8$	��Top=�$
��Left=�$��
175,16,0,0q�$	��120��$	��160��$��1��$��2��._��� -
   $	 kinectPatientFeed2��$��Stretch=�$	��Top=�$
��Fill=�$��-4q�$	��160��$	��120��
 �� $ kinectRegion���� -   $ borderPatient3��$��4q����	#FF01A2E8$	��Top=�$
��Left=�$��	83,16,0,0q�$	��120��$	��160��$��1��$��1��._��� -   $	 kinectPatientFeed3��$��Stretch=�$��Stretch=�$
��Fill=�$��-4q�$	��160��$	��120��
 �� $ kinectRegion���� -   $ borderPatient4��$��4q����	#FF01A2E8$	��Top=�$��Right=�$��
0,16,341,0q�$	��120��$	��160��$��1��$��1��._��� -   $	 kinectPatientFeed4��$��Stretch=�$��Stretch=�$
��Fill=�$��	0,0,-8,-8q�$	��160��$	��120��
 �� $ kinectRegion���� -    ��Name$ patientIDBlock��$
��Left=�$��	4,318,0,0q�$
��Wrap=�$��patient1��$	��Top=�$	��124��$	��356��$
��Bold"�$��64(����	#FF01A2E8
�������$��Microsoft Uighur)�$��1��$��2���� -   $ patientHeartrateBlock��$
��Left=�$��	4,379,0,0q�$
��Wrap=�$	��Top=�$	��124��$	��356��$
��Bold"�$��48(����	#FF01A2E8
�������$��Microsoft Uighur)�$��1��$��2����  � Language$ en-ca�$�Heart rate: 125 bpm���� -   $ patientOxygenSatBlock��$
��Left=�$��	4,427,0,0q�$
��Wrap=�$	��Top=�$��76��$	��356��$
��Bold"�$��48(����	#FF01A2E8
�������$��Microsoft Uighur)�$��1��$��2���� $ en-ca�$�Oxygen Saturation: 99%��8  2Microsoft.Kinect.Toolkit.Controls.KinectTileButton  -     Name$ buttonPatient2��$��Button��$
��Left=�$��
175,16,0,0q�$	��Top=�$	��160��$	��120��
���   � J� Opacity$ 0Q�$��1��$��2��  -   $ buttonPatient3��$
��Left=�$��	83,16,0,0q�$	��Top=�$	��160��$	��120��
���   �$ 0Q�$��1��$��1��  -   $ buttonPatient4��$��Button��$��Right=�$��
0,16,341,0q�$	��Top=�$	��160��$	��120��
���   �$ 0Q�$��1��$��1���� $��	LifeCycle��$��Center=�$��0,-18,-10,0q�$	��Top=�$
��1024��$	��116�����	#FF01A2E8
�������$��Microsoft Uighur)�$��72(�$ 0.49,-0.246&�$��2��$��3���� -   $ borderPatient5��$��4q����	#FF01A2E8$	��Top=�$��Right=�$��
0,16,176,0q�$	��120��$	��160��$��1��$��1��._��� -   $	 kinectPatientFeed5��$��Stretch=�$��Stretch=�$
��Fill=�$��-4q�$	��160��$	��120��
 �� $ kinectRegion��  -   $ buttonPatient5��$��Button��$��Right=�$��
0,16,176,0q�$	��Top=�$	��160��$	��120��
���   �$ 0Q�$��1��$��1���� -   $ borderPatient6��$��4q����	#FF01A2E8$	��Top=�$��Right=�$��	0,16,11,0q�$	��120��$	��160��$��1��$��1��._��� -   $	 kinectPatientFeed6��$��Stretch=�$��Stretch=�$
��Fill=�$��-4q�$	��160��$	��120��
 �� $ kinectRegion��  -   $ buttonPatient6��$��Button��$��Right=�$��	0,16,11,0q�$	��Top=�$	��160��$	��120��
���   �$ 0Q�$��1��$��1��  -   $ buttonPatient1��$��Button��$
��Left=�$��	10,14,0,0q�$	��Top=�$	��160��$	��120�����	#FF0A0202$��1��$ 0Q�