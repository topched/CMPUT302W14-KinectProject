from django.conf.urls import patterns, url

from kinect import views

urlpatterns = patterns('',
	url(r'^$', views.index, name='Login'),
	url(r'^patientstats/', views.patientstats, name='patientstats'),
)