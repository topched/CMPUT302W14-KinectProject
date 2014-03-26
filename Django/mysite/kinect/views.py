from django.shortcuts import render
from django.shortcuts import render_to_response
from django.template import RequestContext, loader
from django_tables2   import RequestConfig
from django.http import HttpResponse
from django.contrib.auth.decorators import login_required
from django.views.decorators.csrf import csrf_exempt
from django.contrib.auth import logout

from kinect.models import Patientstat, Patient
from kinect.tables import PatientTable, NameTable


def login(request):
	context = RequestContext(request)
	return render_to_response('login.html', context)

def main(request):
	context = RequestContext(request)
	if request.user.is_authenticated():
		return render_to_response('main.html', context)
	else:
		return render_to_response('login.html', context)

def patients(request):
	context = RequestContext(request)
	table = NameTable(Patient.objects.all())
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
	context = {}
	fullname = {}
	namesplit = ""
	if request.method == 'POST' :
		fullname = request.POST['name']
		namesplit = fullname.split(",")
		context = Patient.objects.filter(F_name = namesplit[0], L_name = namesplit[1])
		if Patient.objects.filter(F_name = namesplit[0], L_name = namesplit[1]):
			return HttpResponse(context)
		else:
			name = Patient.objects.create(F_name=namesplit[0],L_name=namesplit[1])
			name.save()
			return HttpResponse("yes")
	else:
		return HttpResponse("fail")
	#return render_to_response('data.html', name, date)
	
	
	
	
	


