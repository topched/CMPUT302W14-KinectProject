import django_tables2 as tables
from kinect.models import Patientstat, Patient

class PatientTable(tables.Table):
	class Meta:
		model = Patientstat
		exclude = ("id",)
		attrs = {"class": "paleblue"}

class NameTable(tables.Table):
	class Meta:
		model = Patient
		exclude = ("id",)
		attrs = {"class": "paleblue"}