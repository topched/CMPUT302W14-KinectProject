import datetime
from django.db import models
from django.utils import timezone

class Patient(models.Model):
	F_name = models.CharField(max_length = 30)
	L_name = models.CharField(max_length = 30)
	def __str__(self):
		return self.L_name + ", " + self.F_name
	
		
class Patientstat(models.Model):
	Patients = models.ForeignKey(Patient)
	Date = models.DateTimeField('date published')
	Oxymax = models.CharField(max_length=5)
	Oxymin = models.CharField(max_length=5)
	Oxyavg = models.CharField(max_length=5)
	Hrmax = models.CharField(max_length=5)
	Hrmin = models.CharField(max_length=5)
	Hravg = models.CharField(max_length=5)
	

