from django.conf.urls import patterns, url

from kinect import views

urlpatterns = patterns('',
	url(r'^$', 'django.contrib.auth.views.login', {'template_name': 'login.html'}),
	url(r'^main/', views.main),
	url(r'^patients/(\d+)/', views.patients, name ='PatientStates'),
	url(r'^patientstats/', views.patientstats),
	url(r'^data/', views.data),
	url(r'^logout/', 'django.contrib.auth.views.logout', {'template_name': 'login.html'}),
)