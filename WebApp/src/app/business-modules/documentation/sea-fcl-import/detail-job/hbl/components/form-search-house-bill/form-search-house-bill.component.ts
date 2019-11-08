import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: 'app-form-search-house-bill',
    templateUrl: './form-search-house-bill.component.html',
    styleUrls: ['./form-search-house-bill.component.scss']
})
export class FormSearchHouseBillComponent implements OnInit {
    formSearch: FormGroup;

    constructor() { }

    ngOnInit() {
    }


}
