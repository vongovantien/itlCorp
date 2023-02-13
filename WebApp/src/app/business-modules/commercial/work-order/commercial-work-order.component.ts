import { Component, OnInit } from '@angular/core';
import { CommonEnum } from '@enums';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-commercial-work-order',
    templateUrl: './commercial-work-order.component.html',
})
export class CommercialWorkOrderComponent extends AppList implements OnInit {

    workOrders: any[] = [];

    constructor(
        private readonly _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this.isLoading = of(false);
        this.headers = [
            { title: 'Partner ID', field: 'accountNo', sortable: true },
            { title: 'Name ABBR', field: 'shortName', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },
            { title: 'Salesman', field: 'Saleman', sortable: true },
            { title: 'Agreement Type', field: 'contractType', sortable: true },
        ];
        this.configSearch = {
            settingFields: this.headers.map(x => ({ "fieldName": x.field, "displayName": x.title })),
            typeSearch: CommonEnum.TypeSearch.outtab
        };
    }
}
