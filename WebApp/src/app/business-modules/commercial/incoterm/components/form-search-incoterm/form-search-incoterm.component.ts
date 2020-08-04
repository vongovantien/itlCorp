import { Component, OnInit, Output, EventEmitter, } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { ChargeConstants } from '@constants';
import { Incoterm } from '@models';

@Component({
    selector: 'form-search-incoterm',
    templateUrl: './form-search-incoterm.component.html'
})

export class CommercialFormSearchIncotermComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<Partial<ISearchIncoterm>> = new EventEmitter<Partial<ISearchIncoterm>>();

    formSearch: FormGroup;
    //
    incoterm: AbstractControl;
    service: AbstractControl;
    status: AbstractControl;

    services: CommonInterface.INg2Select[] = [
        { text: 'All', id: 'All' },
        { text: ChargeConstants.IT_DES, id: ChargeConstants.IT_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ];

    statusSelects: CommonInterface.INg2Select[] = [
        { text: 'All', id: 'All' },
        { text: 'Inactive', id: "0" },
        { text: 'Active', id: "1" },
    ];

    constructor(

        private _fb: FormBuilder,
    ) {
        super();
        this.requestSearch = this.onSearchIncoterm;
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.formSearch = this._fb.group({
            incoterm: [],
            service: [[this.services[0]]],
            status: [[this.statusSelects[0]]],
        });
        this.incoterm = this.formSearch.controls["incoterm"];
        this.service = this.formSearch.controls["service"];
        this.status = this.formSearch.controls["status"];
    }

    onSearchIncoterm() {
        const dataForm: { [key: string]: any } = this.formSearch.getRawValue();
        const body = this.formatSearchParams(dataForm);
        this.onSearch.emit(body);

    }
    //
    formatSearchParams(body: any): ISearchIncoterm {
        const checkAllItem = [...body.service].filter(e => e.id === 'All');
        if (checkAllItem.length > 0) {
            return {
                code: body.incoterm,
                service: null,
                active: body.status[0].id === 'All' ? null : body.status[0].id === "1" ? true : false,
            };
        } else {
            const serviceQuery = [...body.service].map(ele => {
                return ele.id;
            });
            return {
                code: body.incoterm,
                service: serviceQuery,
                active: body.status[0].id === 'All' ? null : body.status[0].id === "1" ? true : false,
            };
        }
    }
    //select service
    selelectedService(event) {
        const currService = this.service.value;
        if ([...currService].filter(ele => ele.id === 'All').length > 0 && event.id !== "All") {
            currService.splice(0);
            currService.push(event);
            this.service.setValue(currService);
        }
        if (event.id === 'All') {
            const onlyAllObj = currService.filter(ele => ele.id === 'All');
            this.service.setValue(onlyAllObj);
        }

    }


}
interface ISearchIncoterm {
    code: string;
    service: string[];
    active: boolean;
}