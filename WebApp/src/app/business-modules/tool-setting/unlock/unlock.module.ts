import { NgModule } from '@angular/core';
import { UnlockComponent } from './unlock.component';
import { Route, RouterModule } from '@angular/router';

const routing: Route[] = [
    { path: '', component: UnlockComponent },

];

@NgModule({
    imports: [
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        UnlockComponent,
    ],
    providers: [],
})
export class UnlockModule { }
