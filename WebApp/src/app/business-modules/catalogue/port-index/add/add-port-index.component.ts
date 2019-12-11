import { Component, OnInit } from '@angular/core';
import _map from 'lodash/map';
import { AppList } from 'src/app/app.list';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'app-add-port-index',
    templateUrl: './add-port-index.component.html'
})

export class AddPortIndexComponent extends PopupBase implements OnInit {
    countries: any[] = [];
    countryActive: [] = [];
    areas: [] = [];
    modes: [] = [];
}