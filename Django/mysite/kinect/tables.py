import django_tables2 as tables
from django_tables2.utils import A
from kinect.models import Patientstat, Patient

class PatientTable(tables.Table):
	class Meta:
		model = Patientstat
		exclude = ("id",)
		attrs = {"class": "paleblue"}

class NameTable(tables.Table):
	Name = tables.LinkColumn('PatientStates', args=[A('pk')])
	class Meta:
		model = Patient
		exclude = ("id",)
		attrs = {"class": "paleblue"}