import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingRepo } from '@repositories';
import { catchError, finalize, takeUntil } from 'rxjs/operators';

import { CommonEnum } from '@enums';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountReceivableListTrialOfficialComponent } from '../components/list-trial-official/list-trial-official-account-receivable.component';
import { AccountReceivableListGuaranteedComponent } from '../components/list-guaranteed/list-guaranteed-account-receivable.component';
import { AccountReceivableListOtherComponent } from '../components/list-other/list-other-account-receivable.component';
import { AccountReceivableFormSearchComponent } from '../components/form-search/account-receivable/form-search-account-receivable.component';

@Component({
    selector: 'app-account-receivable',
    templateUrl: './account-receivable.component.html',
})
export class AccountReceivableTabComponent extends AppList implements OnInit {

    //
    @ViewChild(AccountReceivableListTrialOfficialComponent, { static: false }) trialOfficalListComponent: AccountReceivableListTrialOfficialComponent;
    @ViewChild(AccountReceivableListGuaranteedComponent, { static: false }) guaranteedListComponent: AccountReceivableListGuaranteedComponent;
    @ViewChild(AccountReceivableListOtherComponent, { static: false }) otherListComponent: AccountReceivableListOtherComponent;
    //
    @ViewChild(AccountReceivableFormSearchComponent, { static: false }) accountReceivableFormComponent: AccountReceivableFormSearchComponent;

    selectedSubTab: string = "TRIAL_OFFICIAL";

    activeTrialOffice: boolean = false;
    activeGuaranteed: boolean = false;
    activeOther: boolean = false;

    constructor(
        private _router: Router,
        private _activeRouter: ActivatedRoute,
        private _cd: ChangeDetectorRef
    ) {
        super();
    }

    ngOnInit() {
        console.log(1);
    }
    //
    ngAfterViewInit() {
        this._activeRouter
            .queryParams
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((param: { [key: string]: any }) => {
                if (param.subTab) {
                    this.selectedSubTab = param.subTab.toUpperCase();
                } else {
                    this.selectedSubTab = 'trial_official'.toUpperCase();
                    this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
                    //this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
                }

            });
        this._cd.detectChanges();

        console.log(2);
        // if (this.selectedSubTab === null || this.selectedSubTab === 'TRIAL_OFFICIAL') {
        //     this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
        // }

        //this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
    }

    //
    onSearchReceivable(body: AccountingInterface.IAccReceivableSearch) {

        switch (body.arType) {
            case CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical:
                this.setParameterToSearch(body, this.trialOfficalListComponent);
                break;
            case CommonEnum.TabTypeAccountReceivableEnum.Guarantee:
                this.setParameterToSearch(body, this.guaranteedListComponent);
                break;
            case CommonEnum.TabTypeAccountReceivableEnum.Other:
                this.setParameterToSearch(body, this.otherListComponent);
                break;
            default:
                break;
        }
    }
    //
    changeTabAccount(tab: string) {
        if (tab === 'payment') {
            this._router.navigate([`/home/accounting/account-receivable-payable`]);
        }
    }
    //
    setParameterToSearch(dataSearch: AccountingInterface.IAccReceivableSearch, tabComponent: any) {
        tabComponent.dataSearch = dataSearch;
        tabComponent.getPagingList();
    }
    //
    onSelectTabAccountReceivable(tabname: string) {
        console.log(3);
        this.selectedSubTab = tabname;

        if (tabname === 'TRIAL_OFFICIAL') {
            this._router.navigate(['/home/accounting/account-receivable-payable/receivable'], { queryParams: { subTab: 'trial_official' } });

            this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical, this.trialOfficalListComponent);
        } else if (tabname === 'GUARANTEED') {
            this._router.navigate(['/home/accounting/account-receivable-payable/receivable'], { queryParams: { subTab: 'guaranteed' } });

            this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.Guarantee, this.guaranteedListComponent);
        } else {
            this._router.navigate(['/home/accounting/account-receivable-payable/receivable'], { queryParams: { subTab: 'other' } });

            this.setParameterToPagingTab(CommonEnum.TabTypeAccountReceivableEnum.Other, this.otherListComponent);

        }
        this.accountReceivableFormComponent.formSearch.patchValue(Object.assign({}));
        this.accountReceivableFormComponent.initForm();

    }

    setParameterToPagingTab(tab: CommonEnum.TabTypeAccountReceivableEnum, tabComponent: any) {
        console.log(4);
        console.log("tab", tab);
        console.log("tabComponent", tabComponent);

        this.accountReceivableFormComponent.arType = tab;
        //
        const dataSearch: AccountingInterface.IAccReceivableSearch = {
            arType: tab,
            acRefId: null,
            agreementStatus: null,
            agreementExpiredDay: 'All',
            overDueDay: 0,
            salesmanId: null,
            officeId: null,
        };
        tabComponent.dataSearch = dataSearch;
        tabComponent.getPagingList();
    }


}