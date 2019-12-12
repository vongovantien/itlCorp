import { Component, OnInit, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'app-form-port-index',
    templateUrl: './form-port-index.component.html'
})
export class FormPortIndexComponent extends PopupBase implements OnInit {
    portindexForm: FormGroup;
    title: string = '';
    countries: any[] = [];
    areas: any[] = [];
    modes: any[] = [];
    isSubmitted: boolean = false;
    isUpdate: boolean = false;
}