from django.shortcuts import render
from django.shortcuts import render_to_response
from django.http import HttpResponse
from django.template import RequestContext, loader
from kinect.models import Patientstat

def index(request):
	return render_to_response('kinect/login.html')
	
def patientstats(request):
	return render(request, 'kinect/patientstats.html', {"patientstats": Patientstat.objects.all()})

	

# Create your views here.
