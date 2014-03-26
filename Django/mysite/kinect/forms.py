from django import forms

class DataForm(forms.Form):
	name = forms.CharField(max_length=30)
	