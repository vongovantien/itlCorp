import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';

@Component({
    selector: 'form-search-accounting-management',
    templateUrl: './form-search-accounting-management.component.html'
})

export class AccountingManagementFormSearchComponent extends AppForm implements OnInit {

    status: CommonInterface.INg2Select[] = [
        { id: 'New', text: 'New' },
        { id: 'Issued Invoice', text: 'Issued Invoice' },
        { id: 'Issued Voucher', text: 'Issued Voucher' },
    ];

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {
        this.loadPartnerList();
        this.loadUserList();
    }

    loadPartnerList() {
        this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL)
            .subscribe(
                (partners: Partner[]) => {
                }
            );
    }

    loadUserList() {
        this._systemRepo.getSystemUsers({ active: true }).subscribe(
            (users: User) => {
            }
        );
    }
}
