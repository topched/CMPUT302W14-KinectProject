from django.conf.urls import patterns, url

from kinect import views

urlpatterns = patterns('',
	url(r'^$', 'django.contrib.auth.views.login', {'template_name': 'login.html'}),
	url(r'^patientstats/', views.patientstats),
)