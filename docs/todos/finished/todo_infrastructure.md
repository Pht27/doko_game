Okay so the app is running on a Heintzer server and I bought the domain phtanywhere.org.

I have the main branch and the develop branch. I want the workflow to be like this:

I push some changes to the develop branch. Then, github pushes this to the staging / test container. The test container is running on the domain test.phtanywhere.org. When pushing to develop it is important that the service of the prod environment does not get disrupted / interrupted.

I never push to the main branch. I create pull requests in github from develop into main and then merge those. This will push to main and thus should trigger a CI pipeline that pushes to the main app that runs on phtanywhere.org.