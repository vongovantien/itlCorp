import { Component, Input, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'form-search-file-management',
    templateUrl: './form-search-file-management.component.html',
})

export class FormSearchFileManagementComponent extends AppForm implements OnInit {
    @Output() keySearch = new EventEmitter<any>();
    @Input() folderName: string;
    formSearch: FormGroup;
    name: AbstractControl;

    constructor(private _fb: FormBuilder) {
        super();
    }

    ngOnInit() {
        console.log(this.folderName)
        this.formSearch = this._fb.group({
            name: [null],
        })
        this.name = this.formSearch.controls['name'];
    }

    onClickSearch() {
        let valueSearch = { name: this.name.value, folder: this.folderName };
        this.keySearch.emit(valueSearch);
    }

}
