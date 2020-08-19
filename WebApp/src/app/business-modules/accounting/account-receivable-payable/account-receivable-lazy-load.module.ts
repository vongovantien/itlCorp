import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    Acc_Receivable: () => import('./components/tab-account-receivable/tab-account-receivable.module').then(m => m.TabAccountReceivableModule),
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class AccountReceivableLazyLoadModule { }