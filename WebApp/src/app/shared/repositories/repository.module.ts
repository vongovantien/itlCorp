import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo } from './index';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo
    ],
})
export class RepositoryModule {
}