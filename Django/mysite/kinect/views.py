from django.shortcuts import render
from django.shortcuts import render_to_response
from django.template import RequestContext, loader
from django_tables2   import RequestConfig
from django.http import HttpResponse
from django.contrib.auth.decorators import login_required
from django.views.decorators.csrf import csrf_exempt
from django.contrib.auth import logout
from django.utils import timezone
from django.core.exceptions import ObjectDoesNotExist

from kinect.models import Patientstat, Patient
from kinect.tables import PatientTable, NameTable


def login(request):
	context = RequestContext(request)
	return render_to_response('login.html', context)

def main(request):
	context = RequestContext(request)
	table = NameTable(Patient.objects.all())
	RequestConfig(request).configure(table)
	if request.user.is_authenticated():
		return render(request, 'main.html', {'table': table})
	else:
		return render_to_response('login.html', context)

def patients(request, Patient_id):
	context = RequestContext(request)
	table = PatientTable(Patientstat.objects.filter(Patients=Patient_id))
	RequestConfig(request).configure(table)
	if request.user.is_authenticated():
		return render(request, 'patients.html', {'table': table})
	else:
		return render_to_response('login.html', context)	
		
def patientstats(request):
	context = RequestContext(request)
	table = PatientTable(Patientstat.objects.all())
	RequestConfig(request).configure(table)
	if request.user.is_authenticated():
		return render(request, 'patientstats.html', {'table': table})
	else:
		return render_to_response('login.html', context)
	
@csrf_exempt
def data(request):
	if request.method == 'POST' :
		fullname = request.POST['name']
		oxymax = request.POST['OXmax']
		oxymin = request.POST['OXmin']
		oxyavg = request.POST['OXavg']
		hrmax = request.POST['hrmax']
		hrmin = request.POST['hrmin']
		hravg = request.POST['hravg']
		try:
			context = Patient.objects.get(Name = fullname)
		except ObjectDoesNotExist:
			context = Patient.objects.create(Name = fullname)
		Patientstat.objects.create(Patients = context, Date = timezone.now(), Oxymax = oxymax, Oxymin = oxymin, Oxyavg = oxyavg, Hrmax = hrmax, Hrmin = hrmin, Hravg = hravg)
		return HttpResponse(context)
	else:
		return HttpResponse("fail")
	
	
	
	
	


