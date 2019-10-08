import { Component, Output, EventEmitter} from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { User } from 'src/app/shared/models';
import { BaseService } from 'src/app/shared/services';
import { formatDate } from '@angular/common';

@Component({
    selector: 'department-form-search',
    templateUrl: './form-search-department.component.html'
})

export class DepartmentFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchDepartment> = new EventEmitter<ISearchDepartment>();    

    formSearch: FormGroup;
    fieldSearchs: CommonInterface.ICommonTitleValue[] = [];
    selectedTitleFilter: string;
    selectedValueFilter: string;
    searchKey: AbstractControl ;
    //userLogged: User;
    constructor(
        private _fb: FormBuilder,
        private _baseService: BaseService

    ) {
        super();
        this.requestSearch = this.onSubmit;
    }

    ngOnInit() {
        this.initForm();
        this.initDataInform();
    }

    initForm() {
        this.formSearch = this._fb.group({
            searchKey: [],            
        });

        this.searchKey = this.formSearch.controls['searchKey'];
        
    }

    initDataInform() {
        // this.statusApprovals = this.getStatusApproval();
        // this.statusPayments = this.getStatusPayment();
        this.fieldSearchs = this.getFieldSearch();
        this.selectedTitleFilter = this.fieldSearchs[0].title;
        this.selectedValueFilter = this.fieldSearchs[0].value;
    }

    onSubmit() {
        const body: ISearchDepartment = {
            searchOptions: this.selectedValueFilter,
            keyword: this.searchKey.value,            
        };
        this.onSearch.emit(body);
    }

    getFieldSearch(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'All', value: 'All' },
            { title: 'Department Code', value: 'Code' },
            { title: 'Name EN', value: 'NameEN' },
            { title: 'Name Local', value: 'NameLocal' },
            { title: 'Name Abbr', value: 'NameAbbr' },
            { title: 'Office', value: 'Office' },
        ];
    }

    search() {
        this.onSubmit();
    }

    reset() {
        // this.initDataInform();
        // this.resetFormControl(this.requestDate);
        // this.resetFormControl(this.modifiedDate);
        // this.resetFormControl(this.referenceNo);
        // this.resetFormControl(this.paymentMethod);
        // this.resetFormControl(this.statusApproval);

        // this.onSearch.emit(<any>{});
    }
}

interface ISearchDepartment {
    searchOptions: string;
    keyword: string;
}