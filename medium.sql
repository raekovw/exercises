1174. Immediate Food Delivery II

WITH cte AS (
    SELECT
        customer_id,
        order_date,
        RANK() OVER (PARTITION BY customer_id ORDER BY order_date) AS rank,
        customer_pref_delivery_date
    FROM
        Delivery
)
SELECT
    ROUND(
        CAST(100*COUNT(CASE WHEN order_date = customer_pref_delivery_date THEN 1 END) AS decimal) /
        CAST(COUNT(customer_id) AS decimal),2) 
		AS immediate_percentage
FROM
    cte
WHERE 
	rank = 1;


	
------------------------------------------------------------------------------------
1164. Product Price at a Given Date

WITH RankedProducts AS (
    SELECT
        product_id,
        new_price AS price,
        change_date,
        ROW_NUMBER() OVER (PARTITION BY product_id ORDER BY ABS(DATEDIFF(DAY, change_date, '2019-08-16'))) AS RowNum
    FROM
        products
    WHERE
        change_date <= '2019-08-16'
)

SELECT
    product_id,
    price
FROM
    RankedProducts
WHERE
    rownum = 1
UNION

SELECT
    product_id,
    '10' AS price
FROM 
    products
WHERE 
    product_id NOT IN (SELECT product_id FROM RankedProducts)

------------------------------------------------------------------------------------
602. Friend Requests II: Who Has the Most Friends

SELECT TOP 1 
    X.id, 
    count(X.id) as num
FROM (
    SELECT requester_id as id
    FROM RequestAccepted
UNION ALL
    SELECT accepter_id
    FROM RequestAccepted
) X
GROUP BY 
    X.id
ORDER BY 
    num DESC;
	
------------------------------------------------------------------------------------
1907. Count Salary Categories

SELECT 
    b.category,
    COUNT(account_id) AS accounts_count
FROM 
    Accounts a
RIGHT OUTER JOIN 
    (VALUES('Low Salary'),('High Salary'),('Average Salary')) AS b(category) 
    ON CASE 
        WHEN income < 20000 THEN 'Low Salary'
        WHEN income > 50000 THEN 'High Salary'
        ELSE 'Average Salary' END = b.category
GROUP BY 
    b.category

------------------------------------------------------------------------------------
550. Game Play Analysis IV
	
WITH first_login_dates AS 
(
    SELECT player_id, min(event_date) as first_login_date
    FROM Activity
    GROUP BY player_id
)

SELECT 
    ROUND((COUNT(a.player_id)*1.00)/(SELECT COUNT(DISTINCT player_id) FROM Activity) ,2) AS fraction
FROM 
    Activity a 
INNER JOIN 
    first_login_dates fd ON fd.player_id = a.player_id
WHERE 
    DATEDIFF(day, fd.first_login_date, a.event_date) = 1
	
------------------------------------------------------------------------------------
-- 1934. Confirmation Rate
-- SELECT
    -- S.user_id,
    -- ROUND(1.0*SUM(CASE WHEN action ='confirmed' THEN 1 ELSE 0 END)/COUNT(S.user_id),2) AS confirmation_rate
-- FROM 
    -- signups S
-- LEFT JOIN 
    -- confirmations C
-- ON 
    -- S.user_id = C.user_id
-- GROUP BY 
    -- S.user_id
	
------------------------------------------------------------------------------------
-- 1193. Monthly Transactions I

-- SELECT 
    -- FORMAT(trans_date, 'yyyy-MM') AS month,
    -- country,
    -- COUNT(state)AS trans_count,
    -- SUM(IIF(state='approved', 1, 0))AS approved_count,
    -- SUM(amount) AS trans_total_amount,
    -- SUM(IIF(state='approved',amount, 0))AS approved_total_amount 
-- FROM 
    -- transactions
-- GROUP BY 
    -- FORMAT(trans_date, 'yyyy-MM'), country
-- ORDER BY 
    -- month ASC
	
------------------------------------------------------------------------------------
-- 184. Department Highest Salary

-- WITH ranke AS(
-- SELECT
    -- salary,
    -- name,
    -- departmentId,
    -- DENSE_RANK() OVER (PARTITION BY departmentId ORDER BY salary DESC) AS ranked
-- FROM
    -- employee   
-- )

-- SELECT
    -- D.name AS Department, 
    -- R.name AS Employee,
    -- salary AS Salary
-- FROM
    -- ranke R
-- JOIN
    -- Department D ON R.departmentId = D.id
-- WHERE 
    -- ranked = 1
