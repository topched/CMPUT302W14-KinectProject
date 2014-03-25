from django.shortcuts import render
from django.shortcuts import render_to_response
from django.http import HttpResponse
from django.template import RequestContext, loader
from kinect.models import Patientstat

def index(request):
	context = RequestContext(request)
	return render_to_response('login.html', context)
	
def patientstats(request):
	context = RequestContext(request)
	data = {'patientStats': Patientstat.objects.all()}
	return render_to_response('patientstats.html', data, context);

	


